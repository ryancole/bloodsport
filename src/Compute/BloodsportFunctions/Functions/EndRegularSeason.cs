using Azure.Messaging.ServiceBus;

using Bloodsport.Data.Sql;

using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions
{
    public class EndRegularSeason
    {
        private readonly ILogger<EndRegularSeason> _logger;
        private readonly IDbContextFactory<SqlDbContext> _dbFactory;

        public EndRegularSeason(ILogger<EndRegularSeason> logger, IDbContextFactory<SqlDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        [Function(nameof(EndRegularSeason))]
        public async Task Run(
            [ServiceBusTrigger("end-regular-season", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            
        }
    }
}
