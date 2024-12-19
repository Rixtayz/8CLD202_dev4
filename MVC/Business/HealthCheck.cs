using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MVC.Business
{
    public class CustomHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return HealthCheckResult.Healthy("Service is available");
        }
    }
}
