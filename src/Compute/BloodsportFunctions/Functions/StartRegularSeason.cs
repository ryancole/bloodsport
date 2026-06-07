using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class StartRegularSeason
{
    private readonly ILogger<StartRegularSeason> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public StartRegularSeason(ILogger<StartRegularSeason> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function(nameof(StartRegularSeason))]
    public async Task Run(
        [ServiceBusTrigger("start-regular-season", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        StartRegularSeasonMessage? payload;

        try
        {
            payload = JsonSerializer.Deserialize<StartRegularSeasonMessage>(message.Body.ToString());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body: {body}", message.Body);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body could not be deserialized as StartRegularSeasonMessage.");
            return;
        }

        if (payload is null)
        {
            _logger.LogError("Deserialized message payload was null.");
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "InvalidPayload", deadLetterErrorDescription: "Message body deserialized to null.");
            return;
        }

        var seasonId = payload.SeasonId;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season is null)
        {
            _logger.LogError("Season {seasonId} not found.", seasonId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "SeasonNotFound", deadLetterErrorDescription: $"Season {seasonId} does not exist.");
            return;
        }

        season.Status = SeasonStatus.Active;

        await db.SaveChangesAsync();

        _logger.LogInformation("Season {seasonId} status set to Active.", seasonId);

        await messageActions.CompleteMessageAsync(message);
    }
}
