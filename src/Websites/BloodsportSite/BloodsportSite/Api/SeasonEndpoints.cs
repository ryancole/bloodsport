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

            endpoints.MapPost("/seasons/{id}/edit", EditAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapPost("/seasons/{id}/register", RegisterAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            return endpoints;
        }

        // Admin: create a season
        private static async Task<IResult> CreateAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string name,
            [Microsoft.AspNetCore.Mvc.FromForm] DateTime startDate,
            [Microsoft.AspNetCore.Mvc.FromForm] DateTime endDate)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            name = name.Trim();

            if (string.IsNullOrEmpty(name))
                return Results.Redirect("/seasons/create?error=invalid_name");

            if (endDate <= startDate)
                return Results.Redirect("/seasons/create?error=invalid_dates");

            await using var db = dbFactory.CreateDbContext();

            db.Seasons.Add(new Season
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/seasons");
        }

        // Admin: edit a season
        private static async Task<IResult> EditAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] SeasonStatus status,
            [Microsoft.AspNetCore.Mvc.FromForm] bool? registrationOpen = null)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();
            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            season.Status = status;
            season.RegistrationOpen = registrationOpen ?? false;

            await db.SaveChangesAsync();

            return Results.Redirect($"/seasons/{id}");
        }

        // Captain: register their team for a season
        private static async Task<IResult> RegisterAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] long teamId)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect($"/seasons/{id}?error=not_authenticated");

            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            if (!season.RegistrationOpen)
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
