using Microsoft.Extensions.DependencyInjection;
using Argus.Features.Users.Domain.Services;
using Argus.Features.Users.Web.Services;

namespace Argus.Features.Users.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersFeature(this IServiceCollection services)
        {
            // Domain services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserValidator, UserValidator>();

            // Web services
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IUserStateService, UserStateService>();

            return services;
        }
    }
}