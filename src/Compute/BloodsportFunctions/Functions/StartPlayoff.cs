using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class StartPlayoff
{
    private readonly ILogger<StartPlayoff> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public StartPlayoff(ILogger<StartPlayoff> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function(nameof(StartPlayoff))]
    public async Task Run(
        [ServiceBusTrigger("start-playoff", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var payload = JsonSerializer.Deserialize<StartPlayoffMessage>(message.Body.ToString())
            ?? throw new InvalidOperationException("Failed to deserialize StartPlayoffMessage");

        await using var db = _dbFactory.CreateDbContext();

        var playoff = await db.Playoffs
            .Include(p => p.PlayoffRounds)
                .ThenInclude(r => r.PlayoffMatchups)
            .FirstOrDefaultAsync(p => p.Id == payload.PlayoffId);

        if (playoff is null)
        {
            _logger.LogError("Playoff {PlayoffId} not found", payload.PlayoffId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "PlayoffNotFound", deadLetterErrorDescription: $"Playoff {payload.PlayoffId} not found");
            return;
        }

        const int DaysPerRound = 3;
        var now = DateTime.UtcNow;

        var orderedRounds = playoff.PlayoffRounds
            .OrderByDescending(r => r.PlayoffMatchups.Count)
            .ToList();

        for (int i = 0; i < orderedRounds.Count; i++)
        {
            var roundDateEnd = now.AddDays((i + 1) * DaysPerRound);
            orderedRounds[i].DateEnd = roundDateEnd;

            foreach (var matchup in orderedRounds[i].PlayoffMatchups)
                matchup.DateEnd = roundDateEnd;
        }

        playoff.Status = PlayoffStatus.Active;
        await db.SaveChangesAsync();

        await messageActions.CompleteMessageAsync(message);
    }
}
