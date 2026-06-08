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

        var seasonId = payload.SeasonId;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var season = await db.Seasons
            .Include(s => s.TeamSeasonResults)
                .ThenInclude(r => r.Team)
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season is null)
        {
            _logger.LogError("Season {seasonId} not found.", seasonId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "SeasonNotFound", deadLetterErrorDescription: $"Season {seasonId} does not exist.");
            return;
        }

        if (season.Status != SeasonStatus.Active)
        {
            _logger.LogError("Season {seasonId} is not Active (current status: {status}).", seasonId, season.Status);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidSeasonStatus", deadLetterErrorDescription: $"Season must be Active to build a playoff bracket. Current status: {season.Status}.");
            return;
        }

        int totalTeams = season.TeamSeasonResults.Count;

        if (totalTeams < 2)
        {
            _logger.LogError("Season {seasonId} has fewer than 2 team results ({count}).", seasonId, totalTeams);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InsufficientTeams", deadLetterErrorDescription: "At least 2 teams must have season results to build a playoff bracket.");
            return;
        }

        int bracketSize = ComputeBracketSize(totalTeams);
        int roundCount = (int)Math.Log2(bracketSize);

        _logger.LogInformation(
            "Building {bracketSize}-team single-elimination bracket for season {seasonId} ({roundCount} rounds). Total teams: {totalTeams}.",
            bracketSize, seasonId, roundCount, totalTeams);

        var seededTeams = season.TeamSeasonResults
            .OrderByDescending(r => r.WinCount)
            .ThenBy(r => r.LoseCount)
            .Take(bracketSize)
            .ToList();

        // Create all matchup slots for every round, from last round to first so that
        // NextMatchupId FKs can be wired after the first flush (target rows need DB IDs first).
        var allMatchups = new List<PlayoffMatchup>();

        for (int round = roundCount; round >= 1; round--)
        {
            int matchupsInRound = (int)Math.Pow(2, round - 1);

            for (int position = 0; position < matchupsInRound; position++)
            {
                allMatchups.Add(new PlayoffMatchup
                {
                    SeasonId = seasonId,
                    Season = season,
                    Round = round,
                    Position = position,
                });
            }
        }

        db.PlayoffMatchups.AddRange(allMatchups);
        await db.SaveChangesAsync(); // flush to get DB-generated IDs

        // Index by (round, position) for easy lookup.
        var matchupIndex = allMatchups.ToDictionary(m => (m.Round, m.Position));

        // Wire NextMatchupId: a matchup at (round R, position P) feeds into (round R+1, position P/2).
        foreach (var matchup in allMatchups)
        {
            if (matchup.Round == roundCount)
                continue; // Grand Final has no next matchup

            var next = matchupIndex[(matchup.Round + 1, matchup.Position / 2)];
            matchup.NextMatchupId = next.Id;
        }

        // Populate first-round teams using standard seeding: seed 1 vs seed N, seed 2 vs seed N-1, etc.
        // TeamOneId = higher seed (lower index).
        int n = seededTeams.Count;
        for (int i = 0; i < n / 2; i++)
        {
            var matchup = matchupIndex[(1, i)];
            matchup.TeamOneId = seededTeams[i].TeamId;
            matchup.TeamTwoId = seededTeams[n - 1 - i].TeamId;
        }

        season.Status = SeasonStatus.Playoffs;

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "Built {bracketSize}-team bracket ({roundCount} rounds, {matchupCount} total matchups) for season {seasonId}.",
            bracketSize, roundCount, allMatchups.Count, seasonId);

        await messageActions.CompleteMessageAsync(message);
    }

    // Returns the largest power of 2 that is <= floor(totalTeams * 0.20), minimum 2.
    private static int ComputeBracketSize(int totalTeams)
    {
        int qualifierCount = Math.Max((int)Math.Floor(totalTeams * 0.20), 2);
        int size = 1;
        while (size * 2 <= qualifierCount)
            size *= 2;
        return size;
    }
}
