using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class StartRegularSeason
{
    private readonly ILogger<StartRegularSeason> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public StartRegularSeason(ILogger<StartRegularSeason> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
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
            .ToListAsync();

        foreach (var registration in registeredTeams)
        {
            var rosterJson = JsonSerializer.Serialize(new TeamSeasonRosterJson
            {
                AllowedSummonerNames = registration.Team.TeamMemberships
                    .Select(m => $"{m.RiotAccount.GameName}#{m.RiotAccount.TagLine}")
                    .ToList()
            });

            var existing = await db.TeamSeasonRosters
                .FirstOrDefaultAsync(r => r.TeamId == registration.TeamId && r.SeasonId == seasonId);

            if (existing is not null)
            {
                existing.RosterJson = rosterJson;
            }
            else
            {
                db.TeamSeasonRosters.Add(new TeamSeasonRoster
                {
                    TeamId = registration.TeamId,
                    SeasonId = seasonId,
                    Team = registration.Team,
                    Season = season,
                    RosterJson = rosterJson,
                });
            }
        }

        var lastWeek = season.SeasonWeeks.OrderByDescending(w => w.Index).FirstOrDefault();
        if (lastWeek is not null)
            season.EstimatedDateEnd = lastWeek.DateEnd;

        season.Status = SeasonStatus.Active;

        await db.SaveChangesAsync();

        _logger.LogInformation("Season {seasonId} status set to Active with {count} team rosters snapshotted.", seasonId, registeredTeams.Count);

        await messageActions.CompleteMessageAsync(message);
    }
}
