using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class EndPlayoffWeek
{
    private readonly ILogger<EndPlayoffWeek> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public EndPlayoffWeek(ILoggerFactory loggerFactory, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = loggerFactory.CreateLogger<EndPlayoffWeek>();
        _dbFactory = dbFactory;
    }

    [Function("EndPlayoffWeek")]
    public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        
    }
}
