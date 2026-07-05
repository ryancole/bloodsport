using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Caching.Memory;

namespace BloodsportSite.Services
{
    /// <summary>
    /// Azure spend for the configured subscription over a trailing window, fetched from the
    /// Cost Management Query API and cached in-memory so we don't hit ARM on every page load.
    /// </summary>
    public record RecentCost(decimal Amount, string Currency, int Days, DateTimeOffset AsOf);

    public class CostService(
        HttpClient httpClient,
        TokenCredential credential,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<CostService> logger)
    {
        // Trailing window to total spend over.
        private const int WindowDays = 60;

        private const string CacheKey = "cost:recent";
        // Survives the normal TTL and short failure back-offs so a transient 429 doesn't
        // blank the figure; only replaced by a newer successful fetch.
        private const string LastGoodKey = "cost:recent:last-good";

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);
        private static readonly TimeSpan FailureBackoff = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MinRateLimitBackoff = TimeSpan.FromSeconds(30);

        // The scope the ARM token is requested for.
        private static readonly string[] ArmScopes = ["https://management.azure.com/.default"];

        // Azure doesn't bill in real time, so any recent api-version is fine here.
        private const string ApiVersion = "2024-08-01";

        /// <summary>
        /// Returns spend over the trailing <see cref="WindowDays"/> days, or null if it can't be
        /// determined (not configured, no credentials, ARM error). Callers should degrade gracefully.
        /// </summary>
        public async Task<RecentCost?> GetRecentAsync(CancellationToken cancellationToken = default)
        {
            if (cache.TryGetValue(CacheKey, out RecentCost? cached))
            {
                return cached;
            }

            RecentCost? result;
            TimeSpan ttl;

            try
            {
                var (cost, retryAfter) = await QueryAsync(cancellationToken);

                if (retryAfter is { } backoff)
                {
                    // Rate limited. Back off for the advised interval and keep showing the
                    // last good value rather than hammering ARM (which only prolongs the 429).
                    logger.LogWarning(
                        "Cost Management returned 429; backing off for {Seconds:F0}s.",
                        backoff.TotalSeconds);
                    result = LastGood();
                    ttl = backoff;
                }
                else
                {
                    result = cost;
                    ttl = CacheDuration;

                    if (cost is not null)
                    {
                        cache.Set(LastGoodKey, cost);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to query Azure cost for the last {Days} days.", WindowDays);
                result = LastGood();
                ttl = FailureBackoff;
            }

            cache.Set(CacheKey, result, ttl);
            return result;
        }

        private RecentCost? LastGood() =>
            cache.TryGetValue(LastGoodKey, out RecentCost? value) ? value : null;

        /// <summary>
        /// Returns (cost, null) on success, (null, retryAfter) when rate limited, or
        /// (null, null) when there's nothing to query.
        /// </summary>
        private async Task<(RecentCost? Cost, TimeSpan? RetryAfter)> QueryAsync(
            CancellationToken cancellationToken)
        {
            var subscriptionId = configuration["Azure:SubscriptionId"];

            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                logger.LogWarning("Azure:SubscriptionId is not configured; skipping cost query.");
                return (null, null);
            }

            var token = await credential.GetTokenAsync(
                new TokenRequestContext(ArmScopes), cancellationToken);

            var url = $"https://management.azure.com/subscriptions/{subscriptionId}" +
                      $"/providers/Microsoft.CostManagement/query?api-version={ApiVersion}";

            // Trailing WindowDays window, up to the end of today (UTC).
            var todayUtc = DateTime.UtcNow.Date;
            var from = todayUtc.AddDays(-WindowDays);
            var to = todayUtc.AddDays(1).AddSeconds(-1);

            // Single total over a custom period: no grouping, no time granularity.
            var body = new
            {
                type = "ActualCost",
                timeframe = "Custom",
                timePeriod = new
                {
                    from = from.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                    to = to.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                },
                dataset = new
                {
                    granularity = "None",
                    aggregation = new
                    {
                        totalCost = new { name = "Cost", function = "Sum" },
                    },
                },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if ((int)response.StatusCode == 429)
            {
                return (null, ResolveBackoff(response));
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return (Parse(doc.RootElement), null);
        }

        // Cost Management surfaces its throttle window either via the standard Retry-After
        // header or its own x-ms-ratelimit-*-retry-after headers (value in seconds).
        private static readonly string[] RateLimitRetryHeaders =
        [
            "x-ms-ratelimit-microsoft.costmanagement-entity-retry-after",
            "x-ms-ratelimit-microsoft.costmanagement-client-retry-after",
            "x-ms-ratelimit-microsoft.costmanagement-tenant-retry-after",
        ];

        private static TimeSpan ResolveBackoff(HttpResponseMessage response)
        {
            TimeSpan? backoff =
                response.Headers.RetryAfter?.Delta
                ?? (response.Headers.RetryAfter?.Date is { } date ? date - DateTimeOffset.UtcNow : null);

            if (backoff is null)
            {
                foreach (var header in RateLimitRetryHeaders)
                {
                    if (response.Headers.TryGetValues(header, out var values)
                        && int.TryParse(values.FirstOrDefault(), out var seconds))
                    {
                        backoff = TimeSpan.FromSeconds(seconds);
                        break;
                    }
                }
            }

            // Fall back to the minimum, and never back off for less than that.
            return backoff is { } value && value > MinRateLimitBackoff ? value : MinRateLimitBackoff;
        }

        /// <summary>
        /// The query response is a small column/row table. Resolve values by column name
        /// rather than position, since the column order isn't contractually guaranteed.
        /// </summary>
        private static RecentCost? Parse(JsonElement root)
        {
            if (!root.TryGetProperty("properties", out var properties)
                || !properties.TryGetProperty("columns", out var columns)
                || !properties.TryGetProperty("rows", out var rows))
            {
                return null;
            }

            var costIndex = -1;
            var currencyIndex = -1;
            var i = 0;

            foreach (var column in columns.EnumerateArray())
            {
                var name = column.GetProperty("name").GetString();

                if (string.Equals(name, "Cost", StringComparison.OrdinalIgnoreCase))
                {
                    costIndex = i;
                }
                else if (string.Equals(name, "Currency", StringComparison.OrdinalIgnoreCase))
                {
                    currencyIndex = i;
                }

                i++;
            }

            // No rows means no spend recorded in the window.
            var row = rows.EnumerateArray().FirstOrDefault();

            var amount = costIndex >= 0 && row.ValueKind == JsonValueKind.Array
                ? row[costIndex].GetDecimal()
                : 0m;

            var currency = currencyIndex >= 0 && row.ValueKind == JsonValueKind.Array
                ? row[currencyIndex].GetString() ?? "USD"
                : "USD";

            return new RecentCost(amount, currency, WindowDays, DateTimeOffset.UtcNow);
        }
    }
}
