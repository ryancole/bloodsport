using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using BloodsportFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class EndRegularSeasonWeek
{
    private readonly ILogger<EndRegularSeasonWeek> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly EmailService _emailService;

    public EndRegularSeasonWeek(ILoggerFactory loggerFactory, IDbContextFactory<SqlDbContext> dbFactory, EmailService emailService)
    {
        _logger = loggerFactory.CreateLogger<EndRegularSeasonWeek>();
        _dbFactory = dbFactory;
        _emailService = emailService;
    }

    [Function("EndRegularSeasonWeek")]
    public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        _logger.LogInformation("EndRegularSeasonWeek triggered at: {executionTime}", DateTime.UtcNow);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;

        var expiredWeeks = await db.SeasonWeeks
            .Include(w => w.Season)
            .Include(w => w.SeasonWeekMatchups)
                .ThenInclude(m => m.SeasonWeekMatchupResult)
            .Where(w => w.Season.Status == SeasonStatus.Active && w.DateEnd < now && !w.HasBeenEnded)
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

            week.HasBeenEnded = true;
        }

        if (resultsAdded > 0)
            await db.SaveChangesAsync();

        _logger.LogInformation(
            "EndRegularSeasonWeek complete. Checked {weekCount} expired weeks, added {resultCount} DidNotPlay results.",
            expiredWeeks.Count, resultsAdded);

        var endedSeasonIds = expiredWeeks.Select(w => w.SeasonId).Distinct().ToList();

        // Detect seasons where all weeks are now ended and send season-end emails
        var completedSeasonIds = await db.SeasonWeeks
            .Where(w => endedSeasonIds.Contains(w.SeasonId))
            .GroupBy(w => w.SeasonId)
            .Where(g => g.All(w => w.HasBeenEnded))
            .Select(g => g.Key)
            .ToListAsync();

        if (completedSeasonIds.Count > 0)
        {
            var seasonResults = await db.TeamSeasonResults
                .Where(r => completedSeasonIds.Contains(r.SeasonId))
                .Include(r => r.Season)
                .Include(r => r.Team)
                    .ThenInclude(t => t.TeamMemberships)
                        .ThenInclude(tm => tm.RiotAccount)
                            .ThenInclude(a => a.User)
                .ToListAsync();

            var bySeason = seasonResults.GroupBy(r => r.Season);

            foreach (var group in bySeason)
            {
                var teamResults = group.Select(r => (
                    r.Team,
                    r.WinCount,
                    r.LoseCount,
                    r.Team.TeamMemberships.Select(m => m.RiotAccount.User).DistinctBy(u => u.Id)
                ));

                await _emailService.SendSeasonEndedAsync(group.Key, teamResults);
            }
        }

        if (endedSeasonIds.Count > 0)
        {
            var nextWeeks = await db.SeasonWeeks
                .Where(w => endedSeasonIds.Contains(w.SeasonId) && w.DateStart > now && !w.HasBeenEnded)
                .Include(w => w.Season)
                .Include(w => w.SeasonWeekMatchups)
                    .ThenInclude(m => m.TeamOne)
                        .ThenInclude(t => t.TeamMemberships)
                            .ThenInclude(tm => tm.RiotAccount)
                                .ThenInclude(a => a.User)
                .Include(w => w.SeasonWeekMatchups)
                    .ThenInclude(m => m.TeamTwo)
                        .ThenInclude(t => t.TeamMemberships)
                            .ThenInclude(tm => tm.RiotAccount)
                                .ThenInclude(a => a.User)
                .GroupBy(w => w.SeasonId)
                .Select(g => g.OrderBy(w => w.Index).First())
                .ToListAsync();

            foreach (var week in nextWeeks)
            {
                foreach (var matchup in week.SeasonWeekMatchups)
                {
                    var teamOneMembers = matchup.TeamOne.TeamMemberships
                        .Select(m => m.RiotAccount.User)
                        .DistinctBy(u => u.Id);

                    var teamTwoMembers = matchup.TeamTwo.TeamMemberships
                        .Select(m => m.RiotAccount.User)
                        .DistinctBy(u => u.Id);

                    await _emailService.SendMatchupScheduledAsync(week, matchup, matchup.TeamOne, matchup.TeamTwo, teamOneMembers);
                    await _emailService.SendMatchupScheduledAsync(week, matchup, matchup.TeamTwo, matchup.TeamOne, teamTwoMembers);
                }
            }
        }
    }
}
