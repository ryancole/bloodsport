using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions
{
    public class BuildRegularSeason
    {
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
            BuildRegularSeasonMessage? payload;

            try
            {
                payload = JsonSerializer.Deserialize<BuildRegularSeasonMessage>(message.Body.ToString());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message body: {body}", message.Body);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body could not be deserialized as BuildRegularSeasonMessage.");
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
                .Include(s => s.SeasonRegistrations)
                    .ThenInclude(r => r.Team)
                .Include(s => s.SeasonWeeks)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season is null)
            {
                _logger.LogError("Season {seasonId} not found.", seasonId);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "SeasonNotFound", deadLetterErrorDescription: $"Season {seasonId} does not exist.");
                return;
            }

            if (season.RegistrationOpen)
            {
                _logger.LogInformation("Season {seasonId} has registration open. Closing registration before building the schedule.", seasonId);
                season.RegistrationOpen = false;
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

            if (season.EstimatedDateStart is null)
            {
                _logger.LogError("Season {seasonId} is missing estimated start date.", seasonId);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "MissingDates", deadLetterErrorDescription: "Season must have EstimatedDateStart set before building.");
                return;
            }

            _logger.LogInformation("Building {weekCount} weeks for season {seasonId}.", season.Length, seasonId);
            var weeks = BuildWeeks(season);
            db.SeasonWeeks.AddRange(weeks);
            await db.SaveChangesAsync(); // flush so weeks get their DB-generated IDs

            _logger.LogInformation("Building matchups for season {seasonId} across {teamCount} teams.", seasonId, teamCount);
            var teamIds = season.SeasonRegistrations.Select(r => r.TeamId).ToList();
            var teamNames = season.SeasonRegistrations.ToDictionary(r => r.TeamId, r => r.Team.Name);
            var matchups = BuildMatchups(weeks, teamIds, teamNames);
            db.SeasonWeekMatchups.AddRange(matchups);

            var seasonResults = teamIds.Select(teamId => new TeamSeasonResult
            {
                TeamId = teamId,
                SeasonId = seasonId,
                WinCount = 0,
                LoseCount = 0,
            });
            db.TeamSeasonResults.AddRange(seasonResults);

            await db.SaveChangesAsync();

            _logger.LogInformation("Built {weekCount} weeks and {matchupCount} matchups for season {seasonId} across {teamCount} teams.", season.Length, matchups.Count, seasonId, teamCount);

            await messageActions.CompleteMessageAsync(message);
        }

        // Standard round-robin: fix slot 0, rotate slots 1..n-1 clockwise each round.
        // A phantom bye slot (-1) is inserted when team count is odd so pairing math
        // stays uniform; bye pairings are dropped before saving.
        private static List<SeasonWeekMatchup> BuildMatchups(List<SeasonWeek> weeks, List<long> teamIds, Dictionary<long, string> teamNames)
        {
            var slots = new List<long>(teamIds);

            if (slots.Count % 2 != 0)
                slots.Add(-1);

            int n = slots.Count;
            var matchups = new List<SeasonWeekMatchup>();

            for (int weekIndex = 0; weekIndex < weeks.Count; weekIndex++)
            {
                var week = weeks[weekIndex];

                for (int i = 0; i < n / 2; i++)
                {
                    long teamOneId = slots[i];
                    long teamTwoId = slots[n - 1 - i];

                    if (teamOneId == -1 || teamTwoId == -1)
                        continue;

                    matchups.Add(new SeasonWeekMatchup
                    {
                        SeasonWeekId = week.Id,
                        SeasonWeek = week,
                        TeamOneId = teamOneId,
                        TeamTwoId = teamTwoId,
                        Name = $"{teamNames[teamOneId]} vs {teamNames[teamTwoId]}, {week.Name}, {week.Season.Name}",
                    });
                }

                // Rotate slots 1..n-1 one position clockwise; slot 0 stays fixed.
                long last = slots[n - 1];
                slots.RemoveAt(n - 1);
                slots.Insert(1, last);
            }

            return matchups;
        }

        private static List<SeasonWeek> BuildWeeks(Season season)
        {
            if (season.EstimatedDateStart is not DateTime dateStart)
                throw new InvalidOperationException($"Season {season.Id} is missing estimated start date.");

            var weeks = new List<SeasonWeek>();

            for (int i = 0; i < season.Length; i++)
            {
                var weekStart = dateStart.AddDays(7 * i);
                var weekEnd = weekStart.AddDays(7);

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
}
