using System.Net.Http.Json;
using Camille.RiotGames.TournamentV5;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine("Usage: SeedPlayoffMatchupResults --playoff <id> [--url <functionUrl>] [--key <functionKey>] [--skip <0-100>]");
    Console.WriteLine("Connection string is read from appsettings.json or user secrets (ConnectionStrings:Default).");
    Console.WriteLine("Default --url: http://localhost:7071/api/HandlePlayoffMatchup");
    return;
}

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .Build();

long playoffId = 0;
string functionUrl = "http://localhost:7071/api/HandlePlayoffMatchup";
string functionKey = "";
int skipPercent = 20;

for (int i = 0; i < args.Length - 1; i++)
{
    switch (args[i])
    {
        case "--playoff": playoffId   = long.Parse(args[++i]); break;
        case "--url":     functionUrl = args[++i]; break;
        case "--key":     functionKey = args[++i]; break;
        case "--skip":    skipPercent = int.Parse(args[++i]); break;
    }
}

var connString = config.GetConnectionString("Default");

if (playoffId == 0 || string.IsNullOrEmpty(connString))
{
    Console.Error.WriteLine("--playoff is required and ConnectionStrings:Default must be set in appsettings.json or user secrets.");
    Environment.Exit(1);
}

await using var conn = new SqlConnection(connString);
await conn.OpenAsync();

// ── 1. Query playoff matchups without a winner for the playoff ────────────────

var matchups = new List<Matchup>();

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT m.Id, m.TournamentCode, t1.TeamId, t2.TeamId
        FROM PlayoffMatchups m
        INNER JOIN PlayoffRounds r ON r.Id = m.PlayoffRoundId
        LEFT JOIN PlayoffTeams t1 ON t1.Id = m.TeamOneId
        LEFT JOIN PlayoffTeams t2 ON t2.Id = m.TeamTwoId
        WHERE r.PlayoffId = @PlayoffId
          AND m.TeamOneId IS NOT NULL
          AND m.TeamTwoId IS NOT NULL
          AND m.WinningTeamId IS NULL
        """;
    cmd.Parameters.AddWithValue("@PlayoffId", playoffId);

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        matchups.Add(new Matchup(
            reader.GetInt64(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.GetInt64(2),
            reader.GetInt64(3)));
}

if (matchups.Count == 0)
{
    Console.WriteLine($"No pending matchups (without results) found for playoff {playoffId}.");
    return;
}

Console.WriteLine($"Found {matchups.Count} matchup(s) without results.");

// ── 2. Query the season ID for this playoff ───────────────────────────────────

long seasonId = 0;

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT SeasonId FROM Playoffs WHERE Id = @PlayoffId";
    cmd.Parameters.AddWithValue("@PlayoffId", playoffId);
    var result = await cmd.ExecuteScalarAsync();
    if (result is null)
    {
        Console.Error.WriteLine($"Playoff {playoffId} not found.");
        Environment.Exit(1);
    }
    seasonId = (long)result;
}

// ── 3. Query all team member PUUIDs for the season ────────────────────────────

var puuidsByTeam = new Dictionary<long, List<string>>();

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT tm.TeamId, ra.Puuid
        FROM TeamMemberships tm
        INNER JOIN RiotAccounts ra ON ra.Id = tm.RiotAccountId
        INNER JOIN SeasonRegistrations sr ON sr.TeamId = tm.TeamId
        WHERE sr.SeasonId = @SeasonId
        """;
    cmd.Parameters.AddWithValue("@SeasonId", seasonId);

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var teamId = reader.GetInt64(0);
        var puuid = reader.GetString(1);
        if (!puuidsByTeam.TryGetValue(teamId, out var list))
            puuidsByTeam[teamId] = list = [];
        list.Add(puuid);
    }
}

// ── 4. POST a TournamentGamesV5 result for each pending matchup ───────────────

using var http = new HttpClient();
if (!string.IsNullOrEmpty(functionKey))
    http.DefaultRequestHeaders.Add("x-functions-key", functionKey);

var rng = new Random();

foreach (var matchup in matchups)
{
    var tournamentCode = matchup.TournamentCode;

    if (string.IsNullOrEmpty(tournamentCode))
    {
        tournamentCode = $"SEED-PLAYOFF-{matchup.Id}";
        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = "UPDATE PlayoffMatchups SET TournamentCode = @Code WHERE Id = @Id";
        updateCmd.Parameters.AddWithValue("@Code", tournamentCode);
        updateCmd.Parameters.AddWithValue("@Id", matchup.Id);
        await updateCmd.ExecuteNonQueryAsync();
    }

    if (rng.Next(100) < skipPercent)
    {
        Console.WriteLine($"Matchup {matchup.Id} — skipping (reserved for DidNotPlay).");
        continue;
    }

    var winnerId = rng.Next(2) == 0 ? matchup.TeamOneId : matchup.TeamTwoId;
    var loserId  = winnerId == matchup.TeamOneId ? matchup.TeamTwoId : matchup.TeamOneId;

    var winnerPuuids = puuidsByTeam.GetValueOrDefault(winnerId, []);
    var loserPuuids  = puuidsByTeam.GetValueOrDefault(loserId, []);

    if (winnerPuuids.Count == 0 && loserPuuids.Count == 0)
    {
        Console.WriteLine($"Matchup {matchup.Id} ({tournamentCode}): no team members found for either team, skipping.");
        continue;
    }

    var payload = new TournamentGamesV5
    {
        ShortCode   = tournamentCode,
        MetaData    = "",
        GameId      = rng.NextInt64(1_000_000_000L, 9_999_999_999L),
        GameName    = $"seed-game-{matchup.Id}",
        GameType    = "Practice",
        GameMap     = 11,
        GameMode    = "CLASSIC",
        Region      = "NA1",
        StartTime   = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        WinningTeam = winnerPuuids.Select(p => new TournamentTeamV5 { Puuid = p }).ToArray(),
        LosingTeam  = loserPuuids.Select(p => new TournamentTeamV5 { Puuid = p }).ToArray(),
    };

    Console.Write($"Matchup {matchup.Id} ({tournamentCode}) — winner team {winnerId} — posting... ");

    try
    {
        var response = await http.PostAsJsonAsync(functionUrl, payload);
        Console.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed: {ex.Message}");
    }
}

record Matchup(long Id, string? TournamentCode, long TeamOneId, long TeamTwoId);
