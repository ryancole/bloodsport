using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class StartSeason
{
    private readonly ILogger _logger;

    public StartSeason(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<StartSeason>();
    }

    [Function("StartSeason")]
    public void Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}