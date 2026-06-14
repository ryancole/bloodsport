using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class BuildPlayoffBracket
{
    private readonly ILogger<BuildPlayoffBracket> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public BuildPlayoffBracket(ILogger<BuildPlayoffBracket> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function(nameof(BuildPlayoffBracket))]
    public async Task Run(
        [ServiceBusTrigger("build-playoff-bracket", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        BuildPlayoffBracketMessage? payload;

        try
        {
            payload = JsonSerializer.Deserialize<BuildPlayoffBracketMessage>(message.Body.ToString());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body: {body}", message.Body);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body could not be deserialized as BuildPlayoffBracketMessage.");
            return;
        }

        if (payload is null)
        {
            _logger.LogError("Deserialized message payload was null.");
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body deserialized to null.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();

        var playoff = await db.Playoffs
            .Include(p => p.Season)
                .ThenInclude(s => s.TeamSeasonResults)
                    .ThenInclude(r => r.Team)
            .FirstOrDefaultAsync(p => p.Id == payload.PlayoffId);

        if (playoff is null)
        {
            _logger.LogError("Playoff {playoffId} not found.", payload.PlayoffId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "PlayoffNotFound", deadLetterErrorDescription: $"Playoff {payload.PlayoffId} does not exist.");
            return;
        }

        var allTeams = playoff.Season.TeamSeasonResults
            .OrderByDescending(r => r.WinCount)
            .ThenBy(r => r.LoseCount)
            .ToList();

        int totalTeams = allTeams.Count;

        if (totalTeams < 2)
        {
            _logger.LogError("Season {seasonId} has fewer than 2 team results ({count}). Cannot seed a playoff bracket.", playoff.SeasonId, totalTeams);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InsufficientTeams", deadLetterErrorDescription: "At least 2 teams must have season results to build a playoff bracket.");
            return;
        }

        // Take the top 50% of teams, floored to the nearest power of 2.
        // A power-of-2 bracket size is required for clean single-elimination without byes.
        int half = (int)Math.Floor(totalTeams / 2.0);
        int bracketSize = 1;
        while (bracketSize * 2 <= half)
            bracketSize *= 2;

        // Minimum bracket size is 2.
        bracketSize = Math.Max(bracketSize, 2);

        var qualifiedTeams = allTeams.Take(bracketSize).ToList();

        _logger.LogInformation(
            "Playoff {playoffId} (Season {seasonId}): {total} total teams, {count} qualify for playoffs (bracket size: {bracketSize}).",
            playoff.Id, playoff.SeasonId, totalTeams, bracketSize, bracketSize);

        // --- Seed PlayoffTeam records ---

        var playoffTeams = qualifiedTeams
            .Select((result, index) => new PlayoffTeam
            {
                PlayoffId = playoff.Id,
                Playoff = playoff,
                TeamId = result.TeamId,
                Team = result.Team,
                Seed = index + 1,
            })
            .ToList();

        db.PlayoffTeams.AddRange(playoffTeams);
        await db.SaveChangesAsync(); // flush to get DB-generated IDs

        _logger.LogInformation(
            "Inserted {count} PlayoffTeam records for playoff {playoffId}.",
            playoffTeams.Count, playoff.Id);

        // --- Build bracket matchup slots ---
        // Round 1 = grand final (1 matchup). Round roundCount = first round played (bracketSize/2 matchups).
        // Matchups per round: 2^(round - 1).

        int roundCount = (int)Math.Log2(bracketSize);
        const int DaysPerRound = 3;
        var now = DateTime.UtcNow;

        var allMatchups = new List<PlayoffMatchup>();

        for (int round = roundCount; round >= 1; round--)
        {
            int matchupsInRound = (int)Math.Pow(2, round - 1);
            // First round ends in DaysPerRound days; each later round adds DaysPerRound more.
            var dateEnd = now.AddDays((roundCount - round + 1) * DaysPerRound);

            for (int matchNumber = 0; matchNumber < matchupsInRound; matchNumber++)
            {
                allMatchups.Add(new PlayoffMatchup
                {
                    //PlayoffId = playoff.Id,
                    //Playoff = playoff,
                    //Round = round,
                    MatchNumber = matchNumber,
                    DateEnd = dateEnd,
                });
            }
        }

        db.PlayoffMatchups.AddRange(allMatchups);
        await db.SaveChangesAsync(); // flush to get DB-generated IDs

        // --- Wire NextMatchupId ---
        // A matchup at (round R, matchNumber M) advances to (round R-1, matchNumber M/2).
        // Round 1 is the grand final — no next matchup.

        //var matchupIndex = allMatchups.ToDictionary(m => (m.Round, m.MatchNumber));

        //foreach (var matchup in allMatchups)
        //{
        //    if (matchup.Round == 1)
        //        continue;

        //    var next = matchupIndex[(matchup.Round - 1, matchup.MatchNumber / 2)];
        //    matchup.NextMatchupId = next.Id;
        //}

        // --- Seed first-round matchups using standard bracket seeding ---
        // Seed order is generated recursively so top seeds can only meet in later rounds.
        // e.g. for 8 teams: [1,8,4,5,2,7,3,6] → match 0: 1v8, match 1: 4v5, match 2: 2v7, match 3: 3v6.
        // With MatchNumber/2 advancement: semi 0 = (1 or 8) vs (4 or 5), semi 1 = (2 or 7) vs (3 or 6).

        var seedIndex = playoffTeams.ToDictionary(pt => pt.Seed);
        var seedOrder = GenerateBracketSeedOrder(bracketSize);

        //for (int m = 0; m < bracketSize / 2; m++)
        //{
        //    var matchup = matchupIndex[(roundCount, m)];
        //    matchup.TeamOneId = seedIndex[seedOrder[m * 2]].Id;
        //    matchup.TeamTwoId = seedIndex[seedOrder[m * 2 + 1]].Id;
        //}

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "Built {roundCount}-round bracket with {matchupCount} total matchups for playoff {playoffId}.",
            roundCount, allMatchups.Count, playoff.Id);

        await messageActions.CompleteMessageAsync(message);
    }

    // Generates the slot-order array for standard single-elimination seeding.
    // Slots come in pairs: slot 2m and 2m+1 are TeamOne and TeamTwo of match m.
    // e.g. bracketSize=8 → [1,8,4,5,2,7,3,6]
    private static int[] GenerateBracketSeedOrder(int bracketSize)
    {
        var order = new int[] { 1, 2 };
        int size = 2;
        while (size < bracketSize)
        {
            size *= 2;
            var next = new int[size];
            for (int i = 0; i < size / 2; i++)
            {
                next[i * 2] = order[i];
                next[i * 2 + 1] = size + 1 - order[i];
            }
            order = next;
        }
        return order;
    }
}
