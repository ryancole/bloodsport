using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class SeasonEndpoints
    {
        public static IEndpointRouteBuilder MapSeasons(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/seasons/create", CreateAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapPost("/seasons/{id}/register", RegisterAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapPost("/seasons/{id}/generate-matches", GenerateMatchesAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapGet("/seasons/{id}/standings", GetStandingsAsync)
                .RequireAuthorization();

            return endpoints;
        }

        // Admin: create a season
        private static async Task<IResult> CreateAsync(
            HttpContext context,
            SqlDbContext db,
            [Microsoft.AspNetCore.Mvc.FromForm] string name,
            [Microsoft.AspNetCore.Mvc.FromForm] DateTime startDate,
            [Microsoft.AspNetCore.Mvc.FromForm] DateTime endDate)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            name = name.Trim();

            if (string.IsNullOrEmpty(name))
                return Results.BadRequest("Season name is required.");

            if (endDate <= startDate)
                return Results.BadRequest("End date must be after start date.");

            db.Seasons.Add(new Season
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/seasons");
        }

        // Captain: register their team for a season
        private static async Task<IResult> RegisterAsync(
            HttpContext context,
            SqlDbContext db,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] long teamId)
        {
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect($"/seasons/{id}?error=not_authenticated");

            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            if (season.Status != SeasonStatus.Upcoming)
                return Results.Redirect($"/seasons/{id}?error=registration_closed");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.ManagerId == user.Id);

            if (team is null)
                return Results.Redirect($"/seasons/{id}?error=team_not_found");

            var alreadyRegistered = await db.SeasonRegistrations
                .AnyAsync(r => r.SeasonId == id && r.TeamId == teamId);

            if (alreadyRegistered)
                return Results.Redirect($"/seasons/{id}?error=already_registered");

            db.SeasonRegistrations.Add(new SeasonRegistration
            {
                SeasonId = id,
                Season = season,
                TeamId = teamId,
                Team = team,
            });

            await db.SaveChangesAsync();

            return Results.Redirect($"/seasons/{id}");
        }

        // Admin: generate round-robin match schedule for a season
        private static async Task<IResult> GenerateMatchesAsync(
            HttpContext context,
            SqlDbContext db,
            long id)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            var season = await db.Seasons
                .Include(s => s.Registrations)
                    .ThenInclude(r => r.Team)
                .Include(s => s.Matches)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.NotFound("Season not found.");

            if (season.Matches.Count > 0)
                return Results.BadRequest("Matches have already been generated for this season.");

            var teams = season.Registrations.Select(r => r.Team).ToList();

            if (teams.Count < 2)
                return Results.BadRequest("At least 2 teams must be registered.");

            // Standard circle-method round-robin
            // Pad to even number with a "bye" slot (null)
            var slots = teams.Cast<Team?>().ToList();
            if (slots.Count % 2 != 0)
                slots.Add(null);

            int numRounds = slots.Count - 1;
            int half = slots.Count / 2;

            var matches = new List<Match>();

            for (int round = 0; round < numRounds; round++)
            {
                int weekNumber = round + 1;

                for (int i = 0; i < half; i++)
                {
                    var home = slots[i];
                    var away = slots[slots.Count - 1 - i];

                    // Skip bye pairings
                    if (home is null || away is null)
                        continue;

                    matches.Add(new Match
                    {
                        SeasonId = id,
                        Season = season,
                        HomeTeamId = home.Id,
                        HomeTeam = home,
                        AwayTeamId = away.Id,
                        AwayTeam = away,
                        WeekNumber = weekNumber,
                    });
                }

                // Rotate: fix slot[0], rotate the rest clockwise
                var last = slots[slots.Count - 1];
                slots.RemoveAt(slots.Count - 1);
                slots.Insert(1, last);
            }

            db.Matches.AddRange(matches);

            season.Status = SeasonStatus.Active;

            await db.SaveChangesAsync();

            return Results.Redirect($"/seasons/{id}");
        }

        // Get standings for a season
        private static async Task<IResult> GetStandingsAsync(
            SqlDbContext db,
            long id)
        {
            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.NotFound("Season not found.");

            var registrations = await db.SeasonRegistrations
                .Where(r => r.SeasonId == id)
                .Include(r => r.Team)
                .ToListAsync();

            var completedMatches = await db.Matches
                .Where(m => m.SeasonId == id && m.Status == MatchStatus.Completed)
                .ToListAsync();

            var standings = registrations.Select(r =>
            {
                var wins = completedMatches.Count(m => m.WinnerTeamId == r.TeamId);
                var losses = completedMatches.Count(m =>
                    (m.HomeTeamId == r.TeamId || m.AwayTeamId == r.TeamId) &&
                    m.WinnerTeamId != r.TeamId);

                return new
                {
                    TeamId = r.TeamId,
                    TeamName = r.Team.Name,
                    Wins = wins,
                    Losses = losses,
                    Points = wins * 3,
                };
            })
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Wins)
            .ToList();

            return Results.Ok(standings);
        }

        private static async Task<User?> GetCurrentUserAsync(HttpContext context, SqlDbContext db)
        {
            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return null;

            return await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);
        }
    }
}
