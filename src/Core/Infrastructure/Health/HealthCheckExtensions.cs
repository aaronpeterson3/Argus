using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Argus.Core.Infrastructure.Health
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<OrleansHealthCheck>("orleans_health")
                .AddCheck<TenantStoreHealthCheck>("tenant_store_health");

            return services;
        }

        public static void UseHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var result = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalSeconds
                        })
                    };

                    await context.Response.WriteAsJsonAsync(result);
                }
            });
        }
    }
}