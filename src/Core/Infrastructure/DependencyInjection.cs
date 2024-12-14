using Microsoft.Extensions.DependencyInjection;

namespace Argus.Core.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // Add core services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }

    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public interface ICurrentUserService
    {
        string UserId { get; }
        string TenantId { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        public string TenantId => _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
    }
}