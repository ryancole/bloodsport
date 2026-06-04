using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/users/update-display-name", UpdateDisplayNameAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            return endpoints;
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
