using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using BloodsportSite.Services;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class RecruitmentEndpoints
    {
        public static IEndpointRouteBuilder MapRecruitment(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/recruitment/users", ListLookingForTeamAsync);

            return endpoints;
        }

        private static async Task<IResult> ListLookingForTeamAsync(
            IDbContextFactory<SqlDbContext> dbFactory,
            BlobSasService blobSasService,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            UserRecruitmentLanes? lane = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            await using var db = await dbFactory.CreateDbContextAsync();

            var query = db.Users
                .Where(u => u.UserRecruitment != null && u.UserRecruitment.IsLookingForTeam);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(u => u.DisplayName.Contains(search));
            }

            if (lane is not null)
            {
                query = query.Where(u => u.UserRecruitment!.Lanes.Contains(lane.Value));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

            var users = await query
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.UserRecruitment)
                .ToListAsync();

            var items = users.Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.DateCreated,
                FlagUrl = blobSasService.GetSasUrl(u.LogoUrl?.Replace("/original.", "/flag.")),
                Lanes = u.UserRecruitment!.Lanes.ToList(),
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
    }
}
