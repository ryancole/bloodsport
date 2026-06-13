using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class EndPlayoffRound
{
    private readonly ILogger<EndPlayoffRound> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public EndPlayoffRound(ILoggerFactory loggerFactory, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = loggerFactory.CreateLogger<EndPlayoffRound>();
        _dbFactory = dbFactory;
    }

    [Function("EndPlayoffRound")]
    public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        
    }
}
