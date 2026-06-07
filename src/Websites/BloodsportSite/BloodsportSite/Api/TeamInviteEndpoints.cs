using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class TeamInviteEndpoints
    {
        public static IEndpointRouteBuilder MapTeamInvites(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/teams/{teamId}/invite", InviteAsync)
                .RequireAuthorization();

            endpoints.MapPost("/invites/{id}/accept", AcceptAsync)
                .RequireAuthorization();

            endpoints.MapPost("/invites/{id}/decline", DeclineAsync)
                .RequireAuthorization();

            return endpoints;
        }

        private static async Task<IResult> InviteAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long teamId,
            [Microsoft.AspNetCore.Mvc.FromForm] string gameName,
            [Microsoft.AspNetCore.Mvc.FromForm] string tagLine)
        {
            gameName = gameName?.Trim() ?? "";
            tagLine = tagLine?.Trim() ?? "";

            if (string.IsNullOrEmpty(gameName) || string.IsNullOrEmpty(tagLine))
                return Results.Redirect($"/teams/{teamId}?invite_error=invalid_input");

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);
            if (user is null)
                return Results.Redirect($"/teams/{teamId}?invite_error=not_authenticated");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.ManagerId == user.Id);
            if (team is null)
                return Results.Redirect("/teams");

            var riotAccount = await db.RiotAccounts
                .FirstOrDefaultAsync(r => r.GameName == gameName && r.TagLine == tagLine);

            if (riotAccount is null)
                return Results.Redirect($"/teams/{teamId}?invite_error=account_not_found");

            var alreadyMember = await db.TeamMemberships
                .AnyAsync(m => m.TeamId == teamId && m.RiotAccountId == riotAccount.Id);

            if (alreadyMember)
                return Results.Redirect($"/teams/{teamId}?invite_error=already_member");

            var alreadyInvited = await db.TeamInvites
                .AnyAsync(i => i.TeamId == teamId && i.RiotAccountId == riotAccount.Id && i.Status == TeamInviteStatus.Pending);

            if (alreadyInvited)
                return Results.Redirect($"/teams/{teamId}?invite_error=already_invited");

            db.TeamInvites.Add(new TeamInvite
            {
                TeamId = teamId,
                Team = team,
                RiotAccountId = riotAccount.Id,
                RiotAccount = riotAccount,
            });

            await db.SaveChangesAsync();

            return Results.Redirect($"/teams/{teamId}?invite_sent=1");
        }

        private static async Task<IResult> AcceptAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);
            if (user is null)
                return Results.Redirect("/profile");

            var invite = await db.TeamInvites
                .Include(i => i.RiotAccount)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.Id == id && i.Status == TeamInviteStatus.Pending);

            if (invite is null || invite.RiotAccount.UserId != user.Id)
                return Results.Redirect("/profile");

            invite.Status = TeamInviteStatus.Accepted;

            db.TeamMemberships.Add(new TeamMembership
            {
                TeamId = invite.TeamId,
                Team = invite.Team,
                RiotAccountId = invite.RiotAccountId,
                RiotAccount = invite.RiotAccount,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/profile?invite_accepted=1");
        }

        private static async Task<IResult> DeclineAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);
            if (user is null)
                return Results.Redirect("/profile");

            var invite = await db.TeamInvites
                .Include(i => i.RiotAccount)
                .FirstOrDefaultAsync(i => i.Id == id && i.Status == TeamInviteStatus.Pending);

            if (invite is null || invite.RiotAccount.UserId != user.Id)
                return Results.Redirect("/profile");

            invite.Status = TeamInviteStatus.Declined;
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
