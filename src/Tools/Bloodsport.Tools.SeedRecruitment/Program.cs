using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

if (args.Length > 0 && args[0] is "-h" or "--help")
{
    Console.WriteLine("Usage: SeedRecruitment [--count <n>]");
    Console.WriteLine("Marks <n> random teams (default 5) as recruiting a random set of lanes and clears");
    Console.WriteLine("every other team's recruitment, so exactly <n> teams are recruiting at any time.");
    Console.WriteLine("Connection string is read from appsettings.json or user secrets (ConnectionStrings:Default).");
    return;
}

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .Build();

int count = 5;

for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--count")
        count = int.Parse(args[++i]);
}

var connString = config.GetConnectionString("Default");

if (string.IsNullOrEmpty(connString))
{
    Console.Error.WriteLine("ConnectionStrings:Default must be set in appsettings.json or user secrets.");
    Environment.Exit(1);
}

var options = new DbContextOptionsBuilder<SqlDbContext>()
    .UseSqlServer(connString)
    .Options;

await using var db = new SqlDbContext(options);

var teamIds = await db.Teams.Select(t => t.Id).ToListAsync();

if (teamIds.Count == 0)
{
    Console.WriteLine("No teams found.");
    return;
}

var rng = new Random();
count = Math.Min(count, teamIds.Count);

// Pick `count` random teams.
var chosen = teamIds.OrderBy(_ => rng.Next()).Take(count).ToList();

// Clear all existing recruitment so only the chosen teams recruit.
var cleared = await db.TeamRecruitments.ExecuteDeleteAsync();
Console.WriteLine($"Cleared {cleared} existing recruitment row(s).");

var allLanes = Enum.GetValues<TeamRecruitmentLanes>();

foreach (var teamId in chosen)
{
    // A random, non-empty subset of the lanes.
    var lanes = allLanes
        .OrderBy(_ => rng.Next())
        .Take(rng.Next(1, allLanes.Length + 1))
        .OrderBy(l => l)
        .ToList();

    db.TeamRecruitments.Add(new TeamRecruitment
    {
        TeamId = teamId,
        IsLookingForUser = true,
        Lanes = lanes,
    });

    Console.WriteLine($"Team {teamId} — recruiting: {string.Join(", ", lanes)}");
}

await db.SaveChangesAsync();

Console.WriteLine($"Done. {chosen.Count} team(s) now recruiting.");
