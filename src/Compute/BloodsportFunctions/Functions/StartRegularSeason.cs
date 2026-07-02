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

public class StartRegularSeason
{
    private readonly ILogger<StartRegularSeason> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly RiotGamesApi _riotApi;
    private readonly IConfiguration _config;
    private readonly EmailService _emailService;

    public StartRegularSeason(ILogger<StartRegularSeason> logger, IDbContextFactory<SqlDbContext> dbFactory, RiotGamesApi riotApi, IConfiguration config, EmailService emailService)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _riotApi = riotApi;
        _config = config;
        _emailService = emailService;
    }

    [Function(nameof(StartRegularSeason))]
    public async Task Run(
        [ServiceBusTrigger("start-regular-season", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        StartRegularSeasonMessage? payload;

        try
        {
            payload = JsonSerializer.Deserialize<StartRegularSeasonMessage>(message.Body.ToString());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body: {body}", message.Body);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body could not be deserialized as StartRegularSeasonMessage.");
            return;
        }

        if (payload is null)
        {
            _logger.LogError("Deserialized message payload was null.");
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body deserialized to null.");
            return;
        }

        var seasonId = payload.SeasonId;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var season = await db.Seasons
            .Include(s => s.SeasonWeeks)
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season is null)
        {
            _logger.LogError("Season {seasonId} not found.", seasonId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "SeasonNotFound", deadLetterErrorDescription: $"Season {seasonId} does not exist.");
            return;
        }

        var registeredTeams = await db.SeasonRegistrations
            .Where(r => r.SeasonId == seasonId)
            .Include(r => r.Team)
                .ThenInclude(t => t.TeamMemberships)
                    .ThenInclude(m => m.RiotAccount)
                        .ThenInclude(a => a.User)
            .ToListAsync();

        var lastWeek = season.SeasonWeeks.OrderByDescending(w => w.Index).FirstOrDefault();
        if (lastWeek is not null)
            season.EstimatedDateEnd = lastWeek.DateEnd;

        season.Status = SeasonStatus.Active;

        await db.SaveChangesAsync();

        _logger.LogInformation("Season {seasonId} status set to Active.", seasonId);

        var members = registeredTeams
            .SelectMany(r => r.Team.TeamMemberships)
            .Select(m => m.RiotAccount.User)
            .DistinctBy(u => u.Id);

        await _emailService.SendSeasonStartedAsync(season, members);

        // Provision a Riot provider and tournament for this season
        try
        {
            var callbackUrl = _config["RiotApi:RegularSeasonCallback"]
                ?? throw new InvalidOperationException("RiotApi:RegularSeasonCallback is not configured.");

            var route = Enum.Parse<RegionalRoute>(_config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

            long providerId;
            long tournamentId;

            if (RiotApiEndpoints.UseStub)
            {
                providerId = await _riotApi.TournamentStubV5().RegisterProviderDataAsync(route,
                    new StubNs.ProviderRegistrationParametersV5 { Region = season.RiotRegion, Url = callbackUrl });
                tournamentId = await _riotApi.TournamentStubV5().RegisterTournamentAsync(route,
                    new StubNs.TournamentRegistrationParametersV5 { ProviderId = (int)providerId, Name = season.Name });
            }
            else
            {
                providerId = await _riotApi.TournamentV5().RegisterProviderDataAsync(route,
                    new TournNs.ProviderRegistrationParametersV5 { Region = season.RiotRegion, Url = callbackUrl });
                tournamentId = await _riotApi.TournamentV5().RegisterTournamentAsync(route,
                    new TournNs.TournamentRegistrationParametersV5 { ProviderId = (int)providerId, Name = season.Name });
            }

            season.RiotProviderId = providerId;
            season.RiotTournamentId = tournamentId;
            await db.SaveChangesAsync();

            _logger.LogInformation("Provisioned Riot tournament {tournamentId} for season {seasonId}", tournamentId, seasonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision Riot tournament for season {seasonId}. Tournament codes will be unavailable until IDs are set.", seasonId);
        }

        await messageActions.CompleteMessageAsync(message);
    }
}
