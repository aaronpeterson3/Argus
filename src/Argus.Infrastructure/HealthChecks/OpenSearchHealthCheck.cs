using Argus.Infrastructure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Argus.Infrastructure.HealthChecks;

public class OpenSearchHealthCheck : IHealthCheck
{
    private readonly Uri _openSearchUri;

    public OpenSearchHealthCheck(IOptions<OpenSearchOptions> options)
    {
        _openSearchUri = options.Value.Url;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(_openSearchUri, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("OpenSearch is responding normally.");
            }
            
            return HealthCheckResult.Degraded($"OpenSearch returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to OpenSearch", ex);
        }
    }
}