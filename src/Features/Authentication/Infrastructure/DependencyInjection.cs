using Microsoft.Extensions.DependencyInjection;
using Argus.Features.Authentication.Domain.Services;
using Argus.Features.Authentication.Web.Services;

namespace Argus.Features.Authentication.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthenticationFeature(this IServiceCollection services)
        {
            // Domain services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();

            // Web services
            services.AddScoped<IAuthenticationStateProvider, AuthenticationStateProvider>();

            return services;
        }
    }
}