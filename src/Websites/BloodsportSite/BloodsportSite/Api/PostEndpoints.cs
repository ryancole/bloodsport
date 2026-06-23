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

            endpoints.MapPost("/posts/{id}/edit", EditAsync)
                .RequireAuthorization();

            endpoints.MapPost("/posts/{id}/comments", AddCommentAsync)
                .RequireAuthorization();

            endpoints.MapPost("/posts/{id}/comments/{commentId}/delete", DeleteCommentAsync)
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

        private static async Task<IResult> EditAsync(
            long id,
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
                return Results.Redirect($"/news/{id}/edit?error=invalid_title");

            if (string.IsNullOrEmpty(markdown))
                return Results.Redirect($"/news/{id}/edit?error=invalid_content");

            await using var db = dbFactory.CreateDbContext();
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
                return Results.NotFound();

            post.Title = title;
            post.Markdown = markdown;

            await db.SaveChangesAsync();

            return Results.Redirect($"/news/{id}");
        }

        private static async Task<IResult> AddCommentAsync(
            long id,
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string body)
        {
            body = body.Trim();

            if (string.IsNullOrEmpty(body))
                return Results.Redirect($"/news/{id}?comment_error=empty");

            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            await using var db = dbFactory.CreateDbContext();

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);

            if (user is null)
                return Results.Forbid();

            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
                return Results.NotFound();

            db.PostComments.Add(new PostComment
            {
                PostId = id,
                UserId = user.Id,
                Body = body,
                Post = post,
                User = user,
            });

            await db.SaveChangesAsync();

            return Results.Redirect($"/news/{id}");
        }

        private static async Task<IResult> DeleteCommentAsync(
            long id,
            long commentId,
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory)
        {
            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            await using var db = dbFactory.CreateDbContext();

            var comment = await db.PostComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == id);

            if (comment is null)
                return Results.NotFound();

            var isAdmin = context.User.IsInRole("Bloodsport.Admin");
            var isOwner = comment.User.EntraObjectId == oid;

            if (!isAdmin && !isOwner)
                return Results.Forbid();

            db.PostComments.Remove(comment);
            await db.SaveChangesAsync();

            return Results.Redirect($"/news/{id}#comments");
        }
    }
}
