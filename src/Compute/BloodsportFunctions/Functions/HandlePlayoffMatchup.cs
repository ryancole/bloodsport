using System.Text.Json;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Camille.RiotGames.TournamentV5;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class HandlePlayoffMatchup
{
    private readonly ILogger<HandlePlayoffMatchup> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public HandlePlayoffMatchup(ILogger<HandlePlayoffMatchup> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function("HandlePlayoffMatchup")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        return null;
    }
}
