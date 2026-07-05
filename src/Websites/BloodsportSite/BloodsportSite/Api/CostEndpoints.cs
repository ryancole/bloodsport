using BloodsportSite.Services;

namespace BloodsportSite.Api
{
    public static class CostEndpoints
    {
        public static IEndpointRouteBuilder MapCost(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/cost/recent", GetRecentAsync);

            return endpoints;
        }

        private static async Task<IResult> GetRecentAsync(
            CostService costService, CancellationToken cancellationToken)
        {
            var cost = await costService.GetRecentAsync(cancellationToken);

            return cost is null
                ? Results.NoContent()
                : Results.Ok(cost);
        }
    }
}
