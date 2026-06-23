using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class PostEndpoints
    {
        public static IEndpointRouteBuilder MapPosts(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/posts/create", CreateAsync)
                .RequireAuthorization();

            return endpoints;
        }

        private static async Task<IResult> CreateAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string title,
            [Microsoft.AspNetCore.Mvc.FromForm] string markdown)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            title = title.Trim();
            markdown = markdown.Trim();

            if (string.IsNullOrEmpty(title))
                return Results.Redirect("/news/create?error=invalid_title");

            if (string.IsNullOrEmpty(markdown))
                return Results.Redirect("/news/create?error=invalid_content");

            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            await using var db = dbFactory.CreateDbContext();

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);

            if (user is null)
                return Results.Forbid();

            db.Posts.Add(new Post
            {
                UserId = user.Id,
                Title = title,
                Markdown = markdown,
                User = user,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/news");
        }
    }
}
