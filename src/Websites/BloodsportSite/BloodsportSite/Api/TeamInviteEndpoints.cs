using Azure.Communication.Email;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.BlazorForm;
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

            endpoints.MapPost("/teams/{teamId}/apply", ApplyAsync)
                .RequireAuthorization();

            endpoints.MapPost("/invites/{id}/accept", AcceptAsync)
                .RequireAuthorization();

            endpoints.MapPost("/invites/{id}/decline", DeclineAsync)
                .RequireAuthorization();

            endpoints.MapPost("/invites/{id}/cancel", CancelAsync)
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
            [Microsoft.AspNetCore.Mvc.FromForm] TeamInviteForm form)
        {
            var gameName = form.GameName.Trim();
            var tagLine = form.TagLine.Trim();

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
                Status = TeamInviteStatus.Pending,
                Type = TeamInviteType.Invite
            });

            await db.SaveChangesAsync();

            await TrySendInviteEmailAsync(emailClient, templateRenderer, configuration, loggerFactory.CreateLogger(nameof(TeamInviteEndpoints)), riotAccount.User, team, user);

            return Results.Redirect($"/teams/{teamId}?invite_sent=1");
        }

        // User: apply to a team (a player-initiated invite) with one of their own Riot accounts.
        private static async Task<IResult> ApplyAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long teamId,
            [Microsoft.AspNetCore.Mvc.FromForm] TeamApplyForm form)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);
            if (user is null)
                return Results.Unauthorized();

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team is null)
                return Results.NotFound();

            // The Riot account must belong to the current user.
            var riotAccount = await db.RiotAccounts
                .FirstOrDefaultAsync(r => r.Id == form.RiotAccountId && r.UserId == user.Id);

            if (riotAccount is null)
                return Results.BadRequest();

            var alreadyMember = await db.TeamMemberships
                .AnyAsync(m => m.TeamId == teamId && m.RiotAccountId == riotAccount.Id);

            if (alreadyMember)
                return Results.Conflict();

            var alreadyPending = await db.TeamInvites
                .AnyAsync(i => i.TeamId == teamId && i.RiotAccountId == riotAccount.Id && i.Status == TeamInviteStatus.Pending);

            // Idempotent: a pending invite/application already exists, so the client can
            // safely show it as pending.
            if (alreadyPending)
                return Results.Ok();

            db.TeamInvites.Add(new TeamInvite
            {
                TeamId = teamId,
                Team = team,
                RiotAccountId = riotAccount.Id,
                RiotAccount = riotAccount,
                Status = TeamInviteStatus.Pending,
                Type = TeamInviteType.Application
            });

            await db.SaveChangesAsync();

            return Results.Ok();
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

            if (invite is null || !CanRespond(invite, user))
                return Results.Redirect("/profile");

            invite.Status = TeamInviteStatus.Accepted;

            db.TeamMemberships.Add(new TeamMembership
            {
                TeamId = invite.TeamId,
                Team = invite.Team,
                RiotAccountId = invite.RiotAccountId,
                RiotAccount = invite.RiotAccount,
                Active = false
            });

            await db.SaveChangesAsync();

            // Invites are accepted by the invitee (from their profile); applications by the
            // team manager (from the team page).
            return invite.Type == TeamInviteType.Invite
                ? Results.Redirect("/profile?invite_accepted=1")
                : Results.Redirect($"/teams/{invite.TeamId}?application_accepted=1");
        }

        // An invite is answered by the invitee (the Riot account's owner); an application is
        // answered by the manager of the team it was sent to.
        private static bool CanRespond(TeamInvite invite, User user) =>
            invite.Type == TeamInviteType.Invite
                ? invite.RiotAccount.UserId == user.Id
                : invite.Team.ManagerId == user.Id;

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
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.Id == id && i.Status == TeamInviteStatus.Pending);

            if (invite is null || !CanRespond(invite, user))
                return Results.Redirect("/profile");

            invite.Status = TeamInviteStatus.Declined;
            await db.SaveChangesAsync();

            return invite.Type == TeamInviteType.Invite
                ? Results.Redirect("/profile")
                : Results.Redirect($"/teams/{invite.TeamId}");
        }

        // Manager: abort a pending invite their team has sent.
        private static async Task<IResult> CancelAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);
            if (user is null)
                return Results.Redirect("/teams");

            var invite = await db.TeamInvites
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.Id == id && i.Status == TeamInviteStatus.Pending);

            if (invite is null || invite.Team.ManagerId != user.Id)
                return Results.Redirect("/teams");

            var teamId = invite.TeamId;
            db.TeamInvites.Remove(invite);
            await db.SaveChangesAsync();

            return Results.Redirect($"/teams/{teamId}");
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
