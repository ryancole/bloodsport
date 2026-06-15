using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using BloodsportFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class EndPlayoffRound
{
    private readonly ILogger<EndPlayoffRound> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly EmailService _emailService;

    public EndPlayoffRound(ILoggerFactory loggerFactory, IDbContextFactory<SqlDbContext> dbFactory, EmailService emailService)
    {
        _logger = loggerFactory.CreateLogger<EndPlayoffRound>();
        _dbFactory = dbFactory;
        _emailService = emailService;
    }

    [Function("EndPlayoffRound")]
    public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        _logger.LogInformation("EndPlayoffRound triggered at: {executionTime}", DateTime.UtcNow);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;

        var expiredRounds = await db.PlayoffRounds
            .Include(r => r.Playoff)
            .Include(r => r.PlayoffMatchups)
                .ThenInclude(m => m.TeamOne)
                    .ThenInclude(pt => pt.Team)
                        .ThenInclude(t => t.TeamMemberships)
                            .ThenInclude(tm => tm.RiotAccount)
                                .ThenInclude(a => a.User)
            .Include(r => r.PlayoffMatchups)
                .ThenInclude(m => m.TeamTwo)
                    .ThenInclude(pt => pt.Team)
                        .ThenInclude(t => t.TeamMemberships)
                            .ThenInclude(tm => tm.RiotAccount)
                                .ThenInclude(a => a.User)
            .Where(r =>
                r.Playoff.Status == PlayoffStatus.Active &&
                r.DateEnd != null && r.DateEnd < now &&
                r.PlayoffMatchups.Any(m => m.WinningTeamId == null))
            .ToListAsync();

        var defaultedCount = 0;

        foreach (var round in expiredRounds)
        {
            var unresolved = round.PlayoffMatchups.Where(m => m.WinningTeamId == null).ToList();

            foreach (var matchup in unresolved)
            {
                var winner = matchup.TeamOneId ?? matchup.TeamTwoId;

                if (winner is null)
                {
                    _logger.LogWarning(
                        "Playoff matchup {matchupId} in round {roundId} has no teams assigned; skipping.",
                        matchup.Id, round.Id);
                    continue;
                }

                matchup.WinningTeamId = winner;

                if (matchup.NextMatchupId is not null)
                {
                    var nextMatchup = await db.PlayoffMatchups.FirstAsync(m => m.Id == matchup.NextMatchupId);

                    if (matchup.MatchNumber % 2 == 0)
                        nextMatchup.TeamOneId = winner;
                    else
                        nextMatchup.TeamTwoId = winner;
                }

                defaultedCount++;
            }
        }

        if (defaultedCount > 0)
            await db.SaveChangesAsync();

        _logger.LogInformation(
            "EndPlayoffRound complete. Checked {roundCount} expired rounds, defaulted {matchupCount} unresolved matchups.",
            expiredRounds.Count, defaultedCount);

        foreach (var round in expiredRounds)
        {
            var members = round.PlayoffMatchups
                .SelectMany(m => new[] { m.TeamOne, m.TeamTwo })
                .Where(pt => pt is not null)
                .SelectMany(pt => pt!.Team.TeamMemberships)
                .Select(tm => tm.RiotAccount.User)
                .DistinctBy(u => u.Id);

            await _emailService.SendPlayoffRoundEndedAsync(round, members);
        }
    }
}
