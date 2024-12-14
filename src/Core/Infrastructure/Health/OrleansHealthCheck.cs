using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;

namespace Argus.Core.Infrastructure.Health
{
    public class OrleansHealthCheck : IHealthCheck
    {
        private readonly IClusterClient _clusterClient;

        public OrleansHealthCheck(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var management = _clusterClient.GetGrain<IManagementGrain>(0);
                var hosts = await management.GetHosts();

                if (!hosts.Any())
                {
                    return HealthCheckResult.Degraded("No Orleans silos found");
                }

                var activeHosts = hosts.Count(h => h.Status == SiloStatus.Active);
                if (activeHosts == 0)
                {
                    return HealthCheckResult.Unhealthy("No active Orleans silos");
                }

                return HealthCheckResult.Healthy($"Active silos: {activeHosts}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Failed to connect to Orleans cluster", ex);
            }
        }
    }
}