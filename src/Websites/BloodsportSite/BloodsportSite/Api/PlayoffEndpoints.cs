using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class PlayoffEndpoints
    {
        public static IEndpointRouteBuilder MapPlayoffs(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/playoffs/{id}/build-bracket", BuildBracketAsync)
                .RequireAuthorization();

            return endpoints;
        }

        // Admin: queue the BuildPlayoffBracket function for a playoff
        private static async Task<IResult> BuildBracketAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            ServiceBusClient serviceBusClient,
            long id)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();

            var playoff = await db.Playoffs.FirstOrDefaultAsync(p => p.Id == id);

            if (playoff is null)
                return Results.Redirect("/playoffs?error=playoff_not_found");

            await using var sender = serviceBusClient.CreateSender("build-playoff-bracket");

            var payload = JsonSerializer.Serialize(new BuildPlayoffBracketMessage { PlayoffId = id });

            await sender.SendMessageAsync(new ServiceBusMessage(payload));

            return Results.Redirect($"/playoffs/{id}");
        }
    }
}
