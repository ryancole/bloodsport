using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.RiotApi;
using Bloodsport.Entity.ServiceBus;
using BloodsportFunctions.Services;
using Camille.Enums;
using Camille.RiotGames;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StubNs = Camille.RiotGames.TournamentStubV5;
using TournNs = Camille.RiotGames.TournamentV5;

namespace BloodsportFunctions.Functions;

public class StartPlayoff
{
    private readonly ILogger<StartPlayoff> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly RiotGamesApi _riotApi;
    private readonly IConfiguration _config;
    private readonly EmailService _emailService;

    public StartPlayoff(ILogger<StartPlayoff> logger, IDbContextFactory<SqlDbContext> dbFactory, RiotGamesApi riotApi, IConfiguration config, EmailService emailService)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _riotApi = riotApi;
        _config = config;
        _emailService = emailService;
    }

    [Function(nameof(StartPlayoff))]
    public async Task Run(
        [ServiceBusTrigger("start-playoff", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var payload = JsonSerializer.Deserialize<StartPlayoffMessage>(message.Body.ToString())
            ?? throw new InvalidOperationException("Failed to deserialize StartPlayoffMessage");

        await using var db = _dbFactory.CreateDbContext();

        var playoff = await db.Playoffs
            .Include(p => p.PlayoffRounds)
                .ThenInclude(r => r.PlayoffMatchups)
            .FirstOrDefaultAsync(p => p.Id == payload.PlayoffId);

        if (playoff is null)
        {
            _logger.LogError("Playoff {PlayoffId} not found", payload.PlayoffId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "PlayoffNotFound", deadLetterErrorDescription: $"Playoff {payload.PlayoffId} not found");
            return;
        }

        const int DaysPerRound = 3;
        var now = DateTime.UtcNow;

        var orderedRounds = playoff.PlayoffRounds
            .OrderByDescending(r => r.PlayoffMatchups.Count)
            .ToList();

        for (int i = 0; i < orderedRounds.Count; i++)
        {
            var roundDateEnd = now.AddDays((i + 1) * DaysPerRound);
            orderedRounds[i].DateEnd = roundDateEnd;

            foreach (var matchup in orderedRounds[i].PlayoffMatchups)
                matchup.DateEnd = roundDateEnd;
        }

        playoff.Status = PlayoffStatus.Active;
        await db.SaveChangesAsync();

        var playoffTeams = await db.PlayoffTeams
            .Where(pt => pt.PlayoffId == payload.PlayoffId)
            .Include(pt => pt.Team)
                .ThenInclude(t => t.TeamMemberships)
                    .ThenInclude(m => m.RiotAccount)
                        .ThenInclude(a => a.User)
            .ToListAsync();

        var teamsWithMembers = playoffTeams.Select(pt => (
            pt.Team,
            pt.Team.TeamMemberships.Select(m => m.RiotAccount.User).DistinctBy(u => u.Id)
        ));

        await _emailService.SendPlayoffStartedAsync(playoff, teamsWithMembers);

        // Provision a fresh Riot provider + tournament for the playoff
        try
        {
            var callbackUrl = _config["RiotApi:PlayoffCallback"]
                ?? throw new InvalidOperationException("RiotApi:PlayoffCallback is not configured.");

            var route = Enum.Parse<RegionalRoute>(_config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

            long providerId;
            long tournamentId;

            if (RiotApiEndpoints.UseStub)
            {
                providerId = await _riotApi.TournamentStubV5().RegisterProviderDataAsync(route,
                    new StubNs.ProviderRegistrationParametersV5 { Region = playoff.RiotRegion, Url = callbackUrl });
                tournamentId = await _riotApi.TournamentStubV5().RegisterTournamentAsync(route,
                    new StubNs.TournamentRegistrationParametersV5 { ProviderId = (int)providerId, Name = playoff.Name });
            }
            else
            {
                providerId = await _riotApi.TournamentV5().RegisterProviderDataAsync(route,
                    new TournNs.ProviderRegistrationParametersV5 { Region = playoff.RiotRegion, Url = callbackUrl });
                tournamentId = await _riotApi.TournamentV5().RegisterTournamentAsync(route,
                    new TournNs.TournamentRegistrationParametersV5 { ProviderId = (int)providerId, Name = playoff.Name });
            }

            playoff.RiotProviderId = providerId;
            playoff.RiotTournamentId = tournamentId;
            await db.SaveChangesAsync();

            _logger.LogInformation("Provisioned Riot tournament {tournamentId} for playoff {playoffId}", tournamentId, payload.PlayoffId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision Riot tournament for playoff {playoffId}. Tournament codes will be unavailable until IDs are set.", payload.PlayoffId);
        }

        await messageActions.CompleteMessageAsync(message);
    }
}
