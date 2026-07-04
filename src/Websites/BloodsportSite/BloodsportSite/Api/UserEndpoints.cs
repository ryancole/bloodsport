using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.BlazorForm;
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

            endpoints.MapPost("/users/update-recruitment", UpdateRecruitmentAsync)
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

            var query = db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(u => u.DisplayName.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

            var users = await query
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.RiotAccounts)
                    .ThenInclude(a => a.TeamMemberships)
                        .ThenInclude(m => m.Team)
                .ToListAsync();

            var items = users.Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.DateCreated,
                FlagUrl = blobSasService.GetSasUrl(u.LogoUrl?.Replace("/original.", "/flag.")),
                Teams = u.RiotAccounts
                    .SelectMany(a => a.TeamMemberships)
                    .Select(m => m.Team)
                    .DistinctBy(t => t.Id)
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        FlagUrl = blobSasService.GetSasUrl(t.LogoUrl?.Replace("/original.", "/flag.")),
                    }),
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

        private static async Task<IResult> UpdateDisplayNameAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] UpdateDisplayNameForm form)
        {
            var displayName = form.DisplayName.Trim();

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

        private static async Task<IResult> UpdateRecruitmentAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] UpdateRecruitmentForm form)
        {
            var lanes = context.Request.Form["lanes"]
                .Where(v => Enum.TryParse<RiotAccountRecruitmentLanes>(v, out _))
                .Select(Enum.Parse<RiotAccountRecruitmentLanes>)
                .Distinct()
                .OrderBy(l => l)
                .ToList();

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect("/profile");

            // The Riot account must belong to the current user.
            var account = await db.RiotAccounts
                .Include(r => r.RiotAccountRecruitment)
                .FirstOrDefaultAsync(r => r.Id == form.RiotAccountId && r.UserId == user.Id);

            if (account is null)
                return Results.Redirect("/profile");

            var recruitment = account.RiotAccountRecruitment;

            if (recruitment is null)
            {
                recruitment = new RiotAccountRecruitment { RiotAccountId = account.Id };
                db.RiotAccountRecruitments.Add(recruitment);
            }

            recruitment.IsLookingForTeam = form.IsLookingForTeam;
            recruitment.Lanes = form.IsLookingForTeam ? lanes : [];
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
