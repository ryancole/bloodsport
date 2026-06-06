using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Camille.Enums;
using Camille.RiotGames;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class RiotLinkEndpoints
    {
        public static IEndpointRouteBuilder MapRiotLink(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/riot/link", LinkAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            endpoints.MapPost("/riot/unlink", UnlinkAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            return endpoints;
        }

        private static async Task<IResult> LinkAsync(
            HttpContext context,
            IConfiguration config,
            RiotGamesApi riotApi,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string gameName,
            [Microsoft.AspNetCore.Mvc.FromForm] string tagLine)
        {
            gameName = gameName.Trim();
            tagLine = tagLine.Trim().TrimStart('#');

            if (string.IsNullOrEmpty(gameName) || string.IsNullOrEmpty(tagLine))
                return Results.Redirect("/profile?riot_error=invalid_input");

            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return Results.Redirect("/profile?riot_error=not_authenticated");

            await using var db = dbFactory.CreateDbContext();

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);

            if (user is null)
                return Results.Redirect("/profile?riot_error=user_not_found");

            var route = Enum.Parse<RegionalRoute>(config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

            Camille.RiotGames.AccountV1.Account? account;
            try
            {
                account = await riotApi.AccountV1().GetByRiotIdAsync(route, gameName, tagLine);
            }
            catch (Camille.RiotGames.Util.RiotResponseException ex) when (ex.GetResponse().StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.Redirect("/profile?riot_error=not_found");
            }
            catch
            {
                return Results.Redirect("/profile?riot_error=riot_api_failed");
            }

            if (account is null)
                return Results.Redirect("/profile?riot_error=not_found");

            var alreadyLinked = await db.RiotAccounts.AnyAsync(r => r.Puuid == account.Puuid);

            if (alreadyLinked)
                return Results.Redirect("/profile?riot_error=already_linked");

            db.RiotAccounts.Add(new RiotAccount
            {
                UserId = user.Id,
                User = user,
                Puuid = account.Puuid,
                GameName = account.GameName,
                TagLine = account.TagLine,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/profile");
        }

        private static async Task<IResult> UnlinkAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] long riotAccountId)
        {
            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return Results.Redirect("/profile?riot_error=not_authenticated");

            await using var db = dbFactory.CreateDbContext();

            var account = await db.RiotAccounts
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == riotAccountId && r.User.EntraObjectId == oid);

            if (account is null)
                return Results.Redirect("/profile?riot_error=not_found");

            db.RiotAccounts.Remove(account);
            await db.SaveChangesAsync();

            return Results.Redirect("/profile");
        }
    }
}
