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
            endpoints.MapGet("/recruitment/teams", ListRecruitingTeamsAsync);

            return endpoints;
        }

        private static async Task<IResult> ListLookingForTeamAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            BlobSasService blobSasService,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            RiotAccountRecruitmentLanes? lane = null,
            long? teamId = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            await using var db = await dbFactory.CreateDbContextAsync();

            var query = db.RiotAccounts
                .Where(r => r.RiotAccountRecruitment != null && r.RiotAccountRecruitment.IsLookingForTeam);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(r => r.GameName.Contains(search) || r.User.DisplayName.Contains(search));
            }

            if (lane is not null)
            {
                query = query.Where(r => r.RiotAccountRecruitment!.Lanes.Contains(lane.Value));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

            var accounts = await query
                .OrderBy(r => r.GameName)
                .ThenBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.User)
                .Include(r => r.RiotAccountRecruitment)
                .ToListAsync();

            // When a team the caller manages is supplied, flag which of these accounts it has
            // already invited so the client can disable the invite button for them.
            var invitedAccountIds = new HashSet<long>();
            if (teamId is { } tid)
            {
                var oid = context.User.FindFirst("oid")?.Value
                       ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                if (oid is not null && await db.Teams.AnyAsync(t => t.Id == tid && t.Manager.EntraObjectId == oid))
                {
                    var accountIds = accounts.Select(a => a.Id).ToList();
                    invitedAccountIds = (await db.TeamInvites
                        .Where(i => i.TeamId == tid
                                 && i.Status == TeamInviteStatus.Pending
                                 && accountIds.Contains(i.RiotAccountId))
                        .Select(i => i.RiotAccountId)
                        .ToListAsync()).ToHashSet();
                }
            }

            var items = accounts.Select(r => new
            {
                RiotAccountId = r.Id,
                r.GameName,
                r.TagLine,
                UserId = r.UserId,
                r.User.DisplayName,
                FlagUrl = blobSasService.GetSasUrl(r.User.LogoUrl?.Replace("/original.", "/flag.")),
                Lanes = r.RiotAccountRecruitment!.Lanes.ToList(),
                AlreadyInvited = invitedAccountIds.Contains(r.Id),
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

        private static async Task<IResult> ListRecruitingTeamsAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            BlobSasService blobSasService,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            TeamRecruitmentLanes? lane = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            await using var db = await dbFactory.CreateDbContextAsync();

            var query = db.Teams
                .Where(t => t.TeamRecruitment != null && t.TeamRecruitment.IsLookingForUser);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.Name.Contains(search));
            }

            if (lane is not null)
            {
                query = query.Where(t => t.TeamRecruitment!.Lanes.Contains(lane.Value));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

            var teams = await query
                .OrderBy(t => t.Name)
                .ThenBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.TeamRecruitment)
                .ToListAsync();

            // Flag teams the current user already has a pending invite or application with,
            // so the client can show "Application pending" instead of an apply button.
            var pendingTeamIds = new HashSet<long>();
            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is not null)
            {
                var teamIds = teams.Select(t => t.Id).ToList();
                pendingTeamIds = (await db.TeamInvites
                    .Where(i => teamIds.Contains(i.TeamId)
                             && i.Status == TeamInviteStatus.Pending
                             && i.RiotAccount.User.EntraObjectId == oid)
                    .Select(i => i.TeamId)
                    .Distinct()
                    .ToListAsync()).ToHashSet();
            }

            var items = teams.Select(t => new
            {
                t.Id,
                t.Name,
                t.DateCreated,
                FlagUrl = blobSasService.GetSasUrl(t.LogoUrl?.Replace("/original.", "/flag.")),
                Lanes = t.TeamRecruitment!.Lanes.ToList(),
                Pending = pendingTeamIds.Contains(t.Id),
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
