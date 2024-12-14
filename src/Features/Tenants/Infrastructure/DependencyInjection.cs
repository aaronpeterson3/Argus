using Microsoft.Extensions.DependencyInjection;
using Argus.Features.Tenants.Domain.Services;
using Argus.Features.Tenants.Web.Services;

namespace Argus.Features.Tenants.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTenantsFeature(this IServiceCollection services)
        {
            // Domain services
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ITenantValidator, TenantValidator>();

            // Web services
            services.AddScoped<ITenantStateService, TenantStateService>();
            services.AddScoped<ITenantUserService, TenantUserService>();

            return services;
        }
    }
}