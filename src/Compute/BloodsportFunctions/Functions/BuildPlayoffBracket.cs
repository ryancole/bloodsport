using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class BuildPlayoffBracket
{
    private readonly ILogger<BuildPlayoffBracket> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public BuildPlayoffBracket(ILogger<BuildPlayoffBracket> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function(nameof(BuildPlayoffBracket))]
    public async Task Run(
        [ServiceBusTrigger("build-playoff-bracket", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        
    }
}
