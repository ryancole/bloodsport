using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            IHttpClientFactory httpClientFactory,
            SqlDbContext db,
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

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);

            if (user is null)
                return Results.Redirect("/profile?riot_error=user_not_found");

            var apiKey = config["RiotApi:ApiKey"]!;
            var baseUrl = config["RiotApi:BaseUrl"]!;

            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

            var encodedName = Uri.EscapeDataString(gameName);
            var encodedTag = Uri.EscapeDataString(tagLine);
            var response = await httpClient.GetAsync($"{baseUrl}/riot/account/v1/accounts/by-riot-id/{encodedName}/{encodedTag}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Results.Redirect("/profile?riot_error=not_found");

            if (!response.IsSuccessStatusCode)
                return Results.Redirect("/profile?riot_error=riot_api_failed");

            var accountJson = await response.Content.ReadFromJsonAsync<RiotAccountResponse>();

            if (accountJson is null)
                return Results.Redirect("/profile?riot_error=riot_api_failed");

            var alreadyLinked = await db.RiotAccounts.AnyAsync(r => r.Puuid == accountJson.Puuid);

            if (alreadyLinked)
                return Results.Redirect("/profile?riot_error=already_linked");

            db.RiotAccounts.Add(new RiotAccount
            {
                UserId = user.Id,
                User = user,
                Puuid = accountJson.Puuid,
                GameName = accountJson.GameName,
                TagLine = accountJson.TagLine,
            });

            await db.SaveChangesAsync();

            return Results.Redirect("/profile");
        }

        private static async Task<IResult> UnlinkAsync(
            HttpContext context,
            SqlDbContext db,
            [Microsoft.AspNetCore.Mvc.FromForm] long riotAccountId)
        {
            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return Results.Redirect("/profile?riot_error=not_authenticated");

            var account = await db.RiotAccounts
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == riotAccountId && r.User.EntraObjectId == oid);

            if (account is null)
                return Results.Redirect("/profile?riot_error=not_found");

            db.RiotAccounts.Remove(account);
            await db.SaveChangesAsync();

            return Results.Redirect("/profile");
        }

        private sealed class RiotAccountResponse
        {
            [JsonPropertyName("puuid")]
            public required string Puuid { get; init; }

            [JsonPropertyName("gameName")]
            public required string GameName { get; init; }

            [JsonPropertyName("tagLine")]
            public required string TagLine { get; init; }
        }
    }
}
