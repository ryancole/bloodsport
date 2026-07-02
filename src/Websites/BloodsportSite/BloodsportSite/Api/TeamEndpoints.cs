using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.BlazorForm;
using BloodsportSite.Services;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class TeamEndpoints
    {
        public static IEndpointRouteBuilder MapTeams(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/teams", ListAsync);

            endpoints.MapPost("/teams/create", CreateAsync)
                .RequireAuthorization();

            endpoints.MapPost("/teams/{id}/edit", EditAsync)
                .RequireAuthorization();

            endpoints.MapPost("/teams/{id}/delete", DeleteAsync)
                .RequireAuthorization();

            endpoints.MapPost("/teams/{id}/members/{membershipId}/activate", ActivateMemberAsync)
                .RequireAuthorization();

            endpoints.MapPost("/teams/{id}/members/{membershipId}/deactivate", DeactivateMemberAsync)
                .RequireAuthorization();

            return endpoints;
        }

        private static async Task<IResult> ListAsync(
            IDbContextFactory<SqlDbContext> dbFactory,
            BlobSasService blobSasService,
            int page = 1,
            int pageSize = 20,
            string? search = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            await using var db = await dbFactory.CreateDbContextAsync();

            var query = db.Teams.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

            var teams = await query
                .Include(t => t.Manager)
                .OrderBy(t => t.Name)
                .ThenBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = teams.Select(t => new
            {
                t.Id,
                t.Name,
                ManagerId = t.Manager.Id,
                ManagerName = t.Manager.DisplayName,
                t.DateCreated,
                FlagUrl = blobSasService.GetSasUrl(t.LogoUrl?.Replace("/original.", "/flag.")),
                ManagerFlagUrl = blobSasService.GetSasUrl(t.Manager.LogoUrl?.Replace("/original.", "/flag.")),
            });

            return Results.Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages,
            });
        }

        private static async Task<IResult> CreateAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] TeamCreateForm form)
        {
            var name = form.Name.Trim();

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
            TeamLogoService teamLogoService,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] TeamEditForm form,
            IFormFile? logo)
        {
            var name = form.Name.Trim();

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

            if (logo is not null && logo.Length > 0)
            {
                if (logo.Length > 5 * 1024 * 1024)
                    return Results.Redirect($"/teams/{id}/edit?error=logo_too_large");

                var logoUrl = await teamLogoService.UploadLogoAsync(id, logo);
                if (logoUrl is null)
                    return Results.Redirect($"/teams/{id}/edit?error=invalid_logo");
                team.LogoUrl = logoUrl;
            }

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

        private static Task<IResult> ActivateMemberAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            long membershipId)
            => SetMemberActiveAsync(context, dbFactory, id, membershipId, true);

        private static Task<IResult> DeactivateMemberAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            long membershipId)
            => SetMemberActiveAsync(context, dbFactory, id, membershipId, false);

        private static async Task<IResult> SetMemberActiveAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            long membershipId,
            bool active)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect("/teams");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.ManagerId == user.Id);

            if (team is null)
                return Results.Redirect("/teams");

            var membership = await db.TeamMemberships
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.TeamId == id);

            if (membership is null)
                return Results.Redirect($"/teams/{id}");

            if (active && !membership.Active)
            {
                var activeCount = await db.TeamMemberships.CountAsync(m => m.TeamId == id && m.Active);
                if (activeCount >= TeamMembership.MaxActiveMembers)
                    return Results.Redirect($"/teams/{id}?member_error=active_full");
            }

            membership.Active = active;
            await db.SaveChangesAsync();

            return Results.Redirect($"/teams/{id}");
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
