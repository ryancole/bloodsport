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

        var allMatchups = new List<PlayoffMatchup>();

        for (int round = roundCount; round >= 1; round--)
        {
            int matchupsInRound = (int)Math.Pow(2, round - 1);

            for (int matchNumber = 0; matchNumber < matchupsInRound; matchNumber++)
            {
                allMatchups.Add(new PlayoffMatchup
                {
                    PlayoffId = playoff.Id,
                    Playoff = playoff,
                    Round = round,
                    MatchNumber = matchNumber,
                });
            }
        }

        db.PlayoffMatchups.AddRange(allMatchups);
        await db.SaveChangesAsync(); // flush to get DB-generated IDs

        // --- Wire NextMatchupId ---
        // A matchup at (round R, matchNumber M) advances to (round R-1, matchNumber M/2).
        // Round 1 is the grand final — no next matchup.

        var matchupIndex = allMatchups.ToDictionary(m => (m.Round, m.MatchNumber));

        foreach (var matchup in allMatchups)
        {
            if (matchup.Round == 1)
                continue;

            var next = matchupIndex[(matchup.Round - 1, matchup.MatchNumber / 2)];
            matchup.NextMatchupId = next.Id;
        }

        // --- Seed first-round matchups using 1-vs-N method ---
        // First round is round roundCount. Pairing: match M gets seed (M+1) vs seed (bracketSize-M).
        // e.g. for 8 teams: match 0 → seed 1 vs 8, match 1 → seed 2 vs 7, match 2 → seed 3 vs 6, match 3 → seed 4 vs 5.

        var seedIndex = playoffTeams.ToDictionary(pt => pt.Seed);

        for (int m = 0; m < bracketSize / 2; m++)
        {
            var matchup = matchupIndex[(roundCount, m)];
            matchup.TeamOneId = seedIndex[m + 1].Id;
            matchup.TeamTwoId = seedIndex[bracketSize - m].Id;
        }

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "Built {roundCount}-round bracket with {matchupCount} total matchups for playoff {playoffId}.",
            roundCount, allMatchups.Count, playoff.Id);

        await messageActions.CompleteMessageAsync(message);
    }
}
