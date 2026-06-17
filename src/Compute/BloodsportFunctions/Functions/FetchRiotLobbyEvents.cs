using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.RiotApi;
using Camille.Enums;
using Camille.RiotGames;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class FetchRiotLobbyEvents
{
    private readonly ILogger<FetchRiotLobbyEvents> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly RiotGamesApi _riotApi;
    private readonly IConfiguration _config;

    public FetchRiotLobbyEvents(
        ILogger<FetchRiotLobbyEvents> logger,
        IDbContextFactory<SqlDbContext> dbFactory,
        RiotGamesApi riotApi,
        IConfiguration config)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _riotApi = riotApi;
        _config = config;
    }

    private RegionalRoute Route =>
        Enum.Parse<RegionalRoute>(_config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

    [Function(nameof(FetchRiotLobbyEvents))]
    public async Task Run(
        [ServiceBusTrigger("fetch-riot-lobby-events", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var tournamentCode = message.Body.ToString();

        _logger.LogInformation("Fetching lobby events for tournament code {TournamentCode}.", tournamentCode);

        string[] eventJsons;

        try
        {
            if (RiotApiEndpoints.UseStub)
            {
                var result = await _riotApi.TournamentStubV5().GetLobbyEventsByCodeAsync(Route, tournamentCode);
                eventJsons = result.EventList.Select(e => JsonSerializer.Serialize(e)).ToArray();
            }
            else
            {
                var result = await _riotApi.TournamentV5().GetLobbyEventsByCodeAsync(Route, tournamentCode);
                eventJsons = result.EventList.Select(e => JsonSerializer.Serialize(e)).ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch lobby events from Riot API for {TournamentCode}.", tournamentCode);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "RiotApiFailure", deadLetterErrorDescription: ex.Message);
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existing = await db.RiotLobbyEvents
            .Where(e => e.TournamentCode == tournamentCode)
            .ToListAsync();

        db.RiotLobbyEvents.RemoveRange(existing);

        foreach (var eventJson in eventJsons)
        {
            db.RiotLobbyEvents.Add(new RiotLobbyEvent
            {
                TournamentCode = tournamentCode,
                EventJson = eventJson,
            });
        }

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "Saved {Count} lobby event(s) for tournament code {TournamentCode}.",
            eventJsons.Length, tournamentCode);

        await messageActions.CompleteMessageAsync(message);
    }
}
