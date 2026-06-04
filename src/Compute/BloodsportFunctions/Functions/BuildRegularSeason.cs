using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class BuildRegularSeason
{
    private const int WeekCount = 6;

    private readonly ILogger<BuildRegularSeason> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public BuildRegularSeason(ILogger<BuildRegularSeason> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function(nameof(BuildRegularSeason))]
    public async Task Run(
        [ServiceBusTrigger("build-regular-season", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        if (!long.TryParse(message.Body.ToString(), out var seasonId))
        {
            _logger.LogError("Invalid season ID in message body: {body}", message.Body);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body must be a season ID (long).");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();

        var season = await db.Seasons
            .Include(s => s.SeasonRegistrations)
            .Include(s => s.SeasonWeeks)
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season is null)
        {
            _logger.LogError("Season {seasonId} not found.", seasonId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "SeasonNotFound", deadLetterErrorDescription: $"Season {seasonId} does not exist.");
            return;
        }

        if (season.SeasonWeeks.Count > 0)
        {
            _logger.LogWarning("Season {seasonId} already has weeks built. Skipping.", seasonId);
            await messageActions.CompleteMessageAsync(message);
            return;
        }

        int teamCount = season.SeasonRegistrations.Count;

        if (teamCount < 2)
        {
            _logger.LogError("Season {seasonId} has fewer than 2 registered teams ({count}).", seasonId, teamCount);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InsufficientTeams", deadLetterErrorDescription: "At least 2 teams must be registered.");
            return;
        }

        var weeks = BuildWeeks(season, teamCount);

        db.SeasonWeeks.AddRange(weeks);
        await db.SaveChangesAsync();

        _logger.LogInformation("Built {weekCount} weeks for season {seasonId} across {teamCount} teams.", WeekCount, seasonId, teamCount);

        await messageActions.CompleteMessageAsync(message);
    }

    private static List<SeasonWeek> BuildWeeks(Season season, int teamCount)
    {
        var totalDuration = season.EndDate - season.StartDate;
        var weekDuration = TimeSpan.FromTicks(totalDuration.Ticks / WeekCount);

        int baseTeamsPerWeek = teamCount / WeekCount;
        int remainder = teamCount % WeekCount;

        var weeks = new List<SeasonWeek>();

        for (int i = 0; i < WeekCount; i++)
        {
            int teamsThisWeek = baseTeamsPerWeek + (i < remainder ? 1 : 0);

            var weekStart = season.StartDate + TimeSpan.FromTicks(weekDuration.Ticks * i);
            var weekEnd = i == WeekCount - 1 ? season.EndDate : season.StartDate + TimeSpan.FromTicks(weekDuration.Ticks * (i + 1));

            weeks.Add(new SeasonWeek
            {
                SeasonId = season.Id,
                Season = season,
                Index = i + 1,
                Name = $"Week {i + 1}",
                DateStart = weekStart,
                DateEnd = weekEnd,
            });
        }

        return weeks;
    }
}
