using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class TeamEndpoints
    {
        public static IEndpointRouteBuilder MapTeams(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/teams/create", CreateAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapPost("/teams/{id}/edit", EditAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapPost("/teams/{id}/delete", DeleteAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            return endpoints;
        }

        private static async Task<IResult> CreateAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string name)
        {
            name = name.Trim();

            if (string.IsNullOrEmpty(name))
                return Results.Redirect("/teams/create?error=invalid_name");

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect("/teams/create?error=not_authenticated");

            db.Teams.Add(new Team
            {
                Name = name,
                ManagerId = user.Id,
                Manager = user,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/teams");
        }

        private static async Task<IResult> EditAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] string name)
        {
            name = name.Trim();

            if (string.IsNullOrEmpty(name))
                return Results.Redirect($"/teams/{id}/edit?error=invalid_name");

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect($"/teams/{id}/edit?error=not_authenticated");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.ManagerId == user.Id);

            if (team is null)
                return Results.Redirect("/teams");

            team.Name = name;
            await db.SaveChangesAsync();

            return Results.Redirect("/teams");
        }

        private static async Task<IResult> DeleteAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect("/teams");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.ManagerId == user.Id);

            if (team is null)
                return Results.Redirect("/teams");

            db.Teams.Remove(team);
            await db.SaveChangesAsync();

            return Results.Redirect("/teams");
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
