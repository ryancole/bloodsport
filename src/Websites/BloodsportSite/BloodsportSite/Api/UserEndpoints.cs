using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using BloodsportSite.Services;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/users", ListAsync);

            endpoints.MapPost("/users/update-display-name", UpdateDisplayNameAsync)
                .RequireAuthorization();

            endpoints.MapPost("/users/update-logo", UpdateLogoAsync)
                .RequireAuthorization();

            return endpoints;
        }

        private static async Task<IResult> ListAsync(
            IDbContextFactory<SqlDbContext> dbFactory)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var users = await db.Users
                .Include(u => u.RiotAccounts)
                    .ThenInclude(a => a.TeamMemberships)
                        .ThenInclude(m => m.Team)
                .OrderBy(u => u.DisplayName)
                .ToListAsync();

            var result = users.Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.DateCreated,
                Teams = u.RiotAccounts
                    .SelectMany(a => a.TeamMemberships)
                    .Select(m => m.Team)
                    .DistinctBy(t => t.Id)
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        FlagUrl = t.LogoUrl?.Replace("/original.", "/flag."),
                    }),
            });

            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateDisplayNameAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string displayName)
        {
            displayName = displayName.Trim();

            if (string.IsNullOrEmpty(displayName))
                return Results.Redirect("/profile?display_name_error=invalid");

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect("/profile");

            user.DisplayName = displayName;
            await db.SaveChangesAsync();

            return Results.Redirect("/profile");
        }

        private static async Task<IResult> UpdateLogoAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            UserLogoService userLogoService,
            IFormFile? logo)
        {
            if (logo is null || logo.Length == 0)
                return Results.Redirect("/profile?logo_error=invalid_logo");

            if (logo.Length > 5 * 1024 * 1024)
                return Results.Redirect("/profile?logo_error=logo_too_large");

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect("/profile");

            var logoUrl = await userLogoService.UploadLogoAsync(user.Id, logo);
            if (logoUrl is null)
                return Results.Redirect("/profile?logo_error=invalid_logo");

            user.LogoUrl = logoUrl;
            await db.SaveChangesAsync();

            return Results.Redirect("/profile");
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
