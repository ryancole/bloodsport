using System.Net.Http.Json;
using Camille.RiotGames.TournamentV5;
using Microsoft.Data.SqlClient;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine("Usage: SeedMatchupResults --season <id> --server <instance> --database <name> [--url <functionUrl>] [--key <functionKey>] [--skip <0-100>]");
    Console.WriteLine("Default --url: http://localhost:7071/api/HandleSeasonWeekMatchup");
    return;
}

long seasonId = 0;
string server = "";
string database = "";
string functionUrl = "http://localhost:7071/api/HandleSeasonWeekMatchup";
string functionKey = "";
int skipPercent = 20;

for (int i = 0; i < args.Length - 1; i++)
{
    switch (args[i])
    {
        case "--season":   seasonId    = long.Parse(args[++i]); break;
        case "--server":   server      = args[++i]; break;
        case "--database": database    = args[++i]; break;
        case "--url":      functionUrl = args[++i]; break;
        case "--key":      functionKey = args[++i]; break;
        case "--skip":     skipPercent = int.Parse(args[++i]); break;
    }
}

if (seasonId == 0 || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
{
    Console.Error.WriteLine("--season, --server, and --database are required.");
    Environment.Exit(1);
}

var connString = new SqlConnectionStringBuilder
{
    DataSource = server,
    InitialCatalog = database,
    IntegratedSecurity = true,
    TrustServerCertificate = true,
}.ConnectionString;

await using var conn = new SqlConnection(connString);
await conn.OpenAsync();

// ── 1. Query matchups without results for the season ─────────────────────────

var matchups = new List<Matchup>();

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT m.Id, m.TournamentCode, m.TeamOneId, m.TeamTwoId
        FROM SeasonWeekMatchups m
        INNER JOIN SeasonWeeks w ON w.Id = m.SeasonWeekId
        WHERE w.SeasonId = @SeasonId
          AND NOT EXISTS (
              SELECT 1 FROM SeasonWeekMatchupResults r WHERE r.SeasonWeekMatchupId = m.Id
          )
        """;
    cmd.Parameters.AddWithValue("@SeasonId", seasonId);

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
    Console.WriteLine($"No pending matchups (without results) found for season {seasonId}.");
    return;
}

Console.WriteLine($"Found {matchups.Count} matchup(s) without results.");

// ── 2. Query all team member PUUIDs for the season ────────────────────────────

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

// ── 3. POST a TournamentGamesV5 result for each pending matchup ───────────────

using var http = new HttpClient();
if (!string.IsNullOrEmpty(functionKey))
    http.DefaultRequestHeaders.Add("x-functions-key", functionKey);

var rng = new Random();

foreach (var matchup in matchups)
{
    var tournamentCode = matchup.TournamentCode;

    if (string.IsNullOrEmpty(tournamentCode))
    {
        tournamentCode = $"SEED-{matchup.Id}";
        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = "UPDATE SeasonWeekMatchups SET TournamentCode = @Code WHERE Id = @Id";
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
