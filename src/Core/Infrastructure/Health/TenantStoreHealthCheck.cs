using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans.Storage;

namespace Argus.Core.Infrastructure.Health
{
    public class TenantStoreHealthCheck : IHealthCheck
    {
        private readonly IGrainStorage _grainStorage;

        public TenantStoreHealthCheck(IGrainStorage grainStorage)
        {
            _grainStorage = grainStorage;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Attempt to read/write test data
                var testGrainId = $"healthcheck_{Guid.NewGuid()}";
                var testState = new Dictionary<string, object> { { "test", "data" } };

                await _grainStorage.WriteStateAsync(testGrainId, "test", testState);
                await _grainStorage.ReadStateAsync(testGrainId, "test");
                await _grainStorage.ClearStateAsync(testGrainId, "test");

                return HealthCheckResult.Healthy("Tenant store is operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Failed to access tenant store", ex);
            }
        }
    }
}