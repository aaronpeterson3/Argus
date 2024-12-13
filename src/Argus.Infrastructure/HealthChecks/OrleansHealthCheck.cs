using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;

namespace Argus.Infrastructure.HealthChecks;

public class OrleansHealthCheck : IHealthCheck
{
    private readonly IClusterClient _clusterClient;

    public OrleansHealthCheck(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _clusterClient.GetGrain<IManagementGrain>(0).GetHosts();
            return HealthCheckResult.Healthy("Orleans cluster is responding normally.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to Orleans cluster", ex);
        }
    }
}