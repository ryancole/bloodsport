using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace BloodsportSite.Services;

public class AuthenticatedUserTelemetryInitializer(IHttpContextAccessor httpContextAccessor) : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return;

        var oid = user.FindFirst("oid")?.Value
               ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (oid is not null)
            telemetry.Context.User.AuthenticatedUserId = oid;
    }
}
