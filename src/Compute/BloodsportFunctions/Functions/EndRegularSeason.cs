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
    public class EndRegularSeason
    {
        private readonly ILogger<EndRegularSeason> _logger;
        private readonly IDbContextFactory<SqlDbContext> _dbFactory;

        public EndRegularSeason(ILogger<EndRegularSeason> logger, IDbContextFactory<SqlDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        [Function(nameof(EndRegularSeason))]
        public async Task Run(
            [ServiceBusTrigger("end-regular-season", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            EndRegularSeasonMessage? payload;

            try
            {
                payload = JsonSerializer.Deserialize<EndRegularSeasonMessage>(message.Body.ToString());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message body: {body}", message.Body);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body could not be deserialized as EndRegularSeasonMessage.");
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
                .Include(s => s.SeasonWeeks)
                    .ThenInclude(w => w.SeasonWeekMatchups)
                        .ThenInclude(m => m.SeasonWeekMatchupResult)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season is null)
            {
                _logger.LogError("Season {seasonId} not found.", seasonId);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "SeasonNotFound", deadLetterErrorDescription: $"Season {seasonId} does not exist.");
                return;
            }

            if (season.Status == SeasonStatus.Completed)
            {
                _logger.LogWarning("Season {seasonId} is already completed. Skipping.", seasonId);
                await messageActions.CompleteMessageAsync(message);
                return;
            }

            var allMatchups = season.SeasonWeeks
                .SelectMany(w => w.SeasonWeekMatchups)
                .ToList();

            var registeredTeamIds = season.SeasonRegistrations
                .Select(r => r.TeamId)
                .ToList();

            var results = registeredTeamIds.Select(teamId =>
            {
                var teamMatchups = allMatchups
                    .Where(m => m.TeamOneId == teamId || m.TeamTwoId == teamId)
                    .ToList();

                int wins = teamMatchups.Count(m => m.SeasonWeekMatchupResult?.WinnerTeamId == teamId);
                int losses = teamMatchups.Count(m => m.SeasonWeekMatchupResult is not null && m.SeasonWeekMatchupResult.WinnerTeamId != teamId);

                return new TeamSeasonResult
                {
                    TeamId = teamId,
                    SeasonId = seasonId,
                    WinCount = wins,
                    LoseCount = losses,
                };
            }).ToList();

            season.Status = SeasonStatus.Completed;

            db.TeamSeasonResults.AddRange(results);
            await db.SaveChangesAsync();

            _logger.LogInformation("Season {seasonId} marked as completed. Created {count} TeamSeasonResult records.", seasonId, results.Count);

            await messageActions.CompleteMessageAsync(message);
        }
    }
}
