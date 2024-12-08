using Microsoft.Extensions.DependencyInjection;
using Argus.Infrastructure.Data.Interfaces;
using Argus.Infrastructure.Data.Repositories;

namespace Argus.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            
            return services;
        }
    }
}