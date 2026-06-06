using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class HandleMatchupResult
{
    private readonly ILogger<HandleMatchupResult> _logger;

    public HandleMatchupResult(ILogger<HandleMatchupResult> logger)
    {
        _logger = logger;
    }

    [Function("HandleMatchupResult")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}