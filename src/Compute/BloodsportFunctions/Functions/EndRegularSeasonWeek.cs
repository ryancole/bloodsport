using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class EndRegularSeasonWeek
{
    private readonly ILogger<EndRegularSeasonWeek> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public EndRegularSeasonWeek(ILoggerFactory loggerFactory, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = loggerFactory.CreateLogger<EndRegularSeasonWeek>();
        _dbFactory = dbFactory;
    }

    [Function("EndRegularSeasonWeek")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("EndRegularSeasonWeek triggered at: {executionTime}", DateTime.UtcNow);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;

        var expiredWeeks = await db.SeasonWeeks
            .Include(w => w.Season)
            .Include(w => w.SeasonWeekMatchups)
                .ThenInclude(m => m.SeasonWeekMatchupResult)
            .Where(w => w.Season.Status == SeasonStatus.Active && w.DateEnd < now)
            .ToListAsync();

        var resultsAdded = 0;

        foreach (var week in expiredWeeks)
        {
            foreach (var matchup in week.SeasonWeekMatchups)
            {
                if (matchup.SeasonWeekMatchupResult is not null)
                    continue;

                db.SeasonWeekMatchupResults.Add(new SeasonWeekMatchupResult
                {
                    SeasonWeekMatchupId = matchup.Id,
                    SeasonWeekMatchup = matchup,
                    DidNotPlay = true,
                });

                resultsAdded++;
            }
        }

        if (resultsAdded > 0)
            await db.SaveChangesAsync();

        _logger.LogInformation(
            "EndRegularSeasonWeek complete. Checked {weekCount} expired weeks, added {resultCount} DidNotPlay results.",
            expiredWeeks.Count, resultsAdded);
    }
}
