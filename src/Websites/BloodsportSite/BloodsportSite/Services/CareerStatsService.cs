using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Services
{
    /// <summary>
    /// Aggregates all-time, cross-season statistics for teams: playoff champions,
    /// career leaderboards, and per-team career summaries. All figures are derived
    /// from data the league already records (season results + playoff brackets), so
    /// nothing here depends on live Riot data.
    /// </summary>
    public class CareerStatsService(IDbContextFactory<SqlDbContext> dbFactory)
    {
        /// <summary>The champion (and runner-up) of every completed playoff, newest first.</summary>
        public async Task<List<ChampionRecord>> GetChampionsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await GetChampionsAsync(db);
        }

        /// <summary>All-time career stats for every team, ready to rank on a leaderboard.</summary>
        public async Task<List<TeamCareerStats>> GetTeamCareerStatsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var champions = await GetChampionsAsync(db);
            var seasonResults = await db.TeamSeasonResults
                .Include(r => r.Season)
                .ToListAsync();
            var playoffAppearances = await db.PlayoffTeams
                .GroupBy(pt => pt.TeamId)
                .Select(g => new { TeamId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TeamId, x => x.Count);

            var teams = await db.Teams.ToListAsync();

            return teams
                .Select(team => BuildCareerStats(
                    team,
                    seasonResults.Where(r => r.TeamId == team.Id).ToList(),
                    playoffAppearances.GetValueOrDefault(team.Id),
                    champions))
                .Where(s => s.HasHistory)
                .ToList();
        }

        /// <summary>Career stats for a single team, or <c>null</c> if the team has no recorded history.</summary>
        public async Task<TeamCareerStats?> GetTeamCareerStatsAsync(long teamId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team is null)
                return null;

            var champions = await GetChampionsAsync(db);
            var seasonResults = await db.TeamSeasonResults
                .Include(r => r.Season)
                .Where(r => r.TeamId == teamId)
                .ToListAsync();
            var playoffAppearances = await db.PlayoffTeams.CountAsync(pt => pt.TeamId == teamId);

            var stats = BuildCareerStats(team, seasonResults, playoffAppearances, champions);
            return stats.HasHistory ? stats : null;
        }

        private static TeamCareerStats BuildCareerStats(
            Team team,
            List<TeamSeasonResult> seasonResults,
            int playoffAppearances,
            List<ChampionRecord> champions)
        {
            var bestSeason = seasonResults
                .OrderByDescending(r => r.WinCount)
                .ThenBy(r => r.LoseCount)
                .FirstOrDefault();

            return new TeamCareerStats
            {
                TeamId = team.Id,
                TeamName = team.Name,
                LogoUrl = team.LogoUrl,
                SeasonsPlayed = seasonResults.Count,
                RegularSeasonWins = seasonResults.Sum(r => r.WinCount),
                RegularSeasonLosses = seasonResults.Sum(r => r.LoseCount),
                PlayoffAppearances = playoffAppearances,
                Titles = champions.Count(c => c.ChampionTeamId == team.Id),
                RunnerUps = champions.Count(c => c.RunnerUpTeamId == team.Id),
                BestSeasonWins = bestSeason?.WinCount,
                BestSeasonLosses = bestSeason?.LoseCount,
                BestSeasonName = bestSeason?.Season?.Name,
            };
        }

        private static async Task<List<ChampionRecord>> GetChampionsAsync(SqlDbContext db)
        {
            var completedPlayoffs = await db.Playoffs
                .Where(p => p.Status == PlayoffStatus.Completed)
                .Include(p => p.Season)
                .Include(p => p.PlayoffTeams)
                    .ThenInclude(pt => pt.Team)
                .Include(p => p.PlayoffRounds)
                    .ThenInclude(r => r.PlayoffMatchups)
                        .ThenInclude(m => m.PlayoffRoundMatchupResults)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();

            var records = new List<ChampionRecord>();

            foreach (var playoff in completedPlayoffs)
            {
                // The grand final is the terminal matchup (nothing feeds off it). Prefer the
                // round with the fewest matchups so a third-place match can't be mistaken for it.
                var finalMatchup = playoff.PlayoffRounds
                    .OrderBy(r => r.PlayoffMatchups.Count)
                    .SelectMany(r => r.PlayoffMatchups)
                    .FirstOrDefault(m => m.NextMatchupId is null);

                var result = finalMatchup?.PlayoffRoundMatchupResults
                    .FirstOrDefault(r => r.WinningTeamId is not null);

                // PlayoffRoundMatchup team ids and the result's winning id both reference PlayoffTeam.
                PlayoffTeam? championPt = null;
                PlayoffTeam? runnerUpPt = null;

                if (finalMatchup is not null && result?.WinningTeamId is { } winnerPtId)
                {
                    championPt = playoff.PlayoffTeams.FirstOrDefault(pt => pt.Id == winnerPtId);

                    var runnerUpPtId = finalMatchup.TeamOneId == winnerPtId
                        ? finalMatchup.TeamTwoId
                        : finalMatchup.TeamOneId;
                    runnerUpPt = playoff.PlayoffTeams.FirstOrDefault(pt => pt.Id == runnerUpPtId);
                }

                records.Add(new ChampionRecord
                {
                    PlayoffId = playoff.Id,
                    PlayoffName = playoff.Name,
                    SeasonId = playoff.SeasonId,
                    SeasonName = playoff.Season?.Name ?? "Unknown Season",
                    DateCreated = playoff.DateCreated,
                    ChampionTeamId = championPt?.TeamId,
                    ChampionTeamName = championPt?.Team?.Name,
                    ChampionLogoUrl = championPt?.Team?.LogoUrl,
                    RunnerUpTeamId = runnerUpPt?.TeamId,
                    RunnerUpTeamName = runnerUpPt?.Team?.Name,
                    RunnerUpLogoUrl = runnerUpPt?.Team?.LogoUrl,
                });
            }

            return records;
        }
    }

    public class ChampionRecord
    {
        public required long PlayoffId { get; init; }
        public required string PlayoffName { get; init; }
        public required long SeasonId { get; init; }
        public required string SeasonName { get; init; }
        public required DateTime DateCreated { get; init; }

        public long? ChampionTeamId { get; init; }
        public string? ChampionTeamName { get; init; }
        public string? ChampionLogoUrl { get; init; }

        public long? RunnerUpTeamId { get; init; }
        public string? RunnerUpTeamName { get; init; }
        public string? RunnerUpLogoUrl { get; init; }
    }

    public class TeamCareerStats
    {
        public required long TeamId { get; init; }
        public required string TeamName { get; init; }
        public string? LogoUrl { get; init; }

        public required int SeasonsPlayed { get; init; }
        public required int RegularSeasonWins { get; init; }
        public required int RegularSeasonLosses { get; init; }
        public required int PlayoffAppearances { get; init; }
        public required int Titles { get; init; }
        public required int RunnerUps { get; init; }

        public int? BestSeasonWins { get; init; }
        public int? BestSeasonLosses { get; init; }
        public string? BestSeasonName { get; init; }

        public int RegularSeasonGames => RegularSeasonWins + RegularSeasonLosses;

        public double? WinPct => RegularSeasonGames == 0
            ? null
            : (double)RegularSeasonWins / RegularSeasonGames;

        /// <summary>Whether the team has any recorded season or playoff history worth listing.</summary>
        public bool HasHistory => SeasonsPlayed > 0 || PlayoffAppearances > 0;
    }
}
