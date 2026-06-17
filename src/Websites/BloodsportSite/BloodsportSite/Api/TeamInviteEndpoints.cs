using Azure.Communication.Email;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Common.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            EmailClient emailClient,
            EmailTemplateRenderer templateRenderer,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
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
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.GameName == gameName && r.TagLine == tagLine);

            if (riotAccount is null)
                return Results.Redirect($"/teams/{teamId}?invite_error=account_not_found");

            var memberCount = await db.TeamMemberships.CountAsync(m => m.TeamId == teamId);
            if (memberCount >= 7)
                return Results.Redirect($"/teams/{teamId}?invite_error=roster_full");

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

            await TrySendInviteEmailAsync(emailClient, templateRenderer, configuration, loggerFactory.CreateLogger(nameof(TeamInviteEndpoints)), riotAccount.User, team, user);

            return Results.Redirect($"/teams/{teamId}?invite_sent=1");
        }

        private static async Task TrySendInviteEmailAsync(EmailClient emailClient, EmailTemplateRenderer templateRenderer, IConfiguration configuration, ILogger logger, User invitee, Team team, User manager)
        {
            var senderAddress = configuration["Email:SenderAddress"];
            if (senderAddress is null || string.IsNullOrEmpty(invitee.Email))
                return;

            var html = await templateRenderer.RenderAsync("TeamInvite.html", new { manager_name = manager.DisplayName, team_name = team.Name });

            var content = new EmailContent($"You've been invited to join {team.Name}")
            {
                PlainText = $"{manager.DisplayName} has invited you to join {team.Name}. Head to the site and check your profile to accept or decline.",
                Html = html
            };

            var message = new EmailMessage(
                senderAddress: senderAddress,
                recipients: new EmailRecipients([new EmailAddress(invitee.Email)]),
                content: content);

            try
            {
                await emailClient.SendAsync(Azure.WaitUntil.Started, message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send team invite email to user {UserId}.", invitee.Id);
            }
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

            var memberCount = await db.TeamMemberships.CountAsync(m => m.TeamId == invite.TeamId);
            if (memberCount >= 7)
                return Results.Redirect("/profile?invite_error=roster_full");

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
