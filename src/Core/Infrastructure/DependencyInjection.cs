using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Argus.Core.Infrastructure.Caching;
using Argus.Core.Infrastructure.FeatureFlags;
using Argus.Core.Infrastructure.BackgroundJobs;
using Argus.Core.Infrastructure.Events;

namespace Argus.Core.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Common services
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();

            // Add Redis Cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "Argus:";
            });

            // Add Caching Services
            services.AddScoped<ICacheService, RedisCacheService>();

            // Add Feature Flags
            services.AddScoped<IFeatureManager, FeatureManager>();

            // Add Background Jobs
            services.AddScoped<IJobService, JobService>();

            // Add Event Publishing
            services.AddScoped<IEventPublisher, EventPublisher>();

            // Configure Orleans Streaming
            services.AddOrleansStreaming(configuration);

            return services;
        }

        private static IServiceCollection AddOrleansStreaming(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrleans(siloBuilder =>
            {
                siloBuilder.AddMemoryStreams("EventStream")
                    .AddMemoryGrainStorage("PubSubStore");

                if (!string.IsNullOrEmpty(configuration.GetConnectionString("AzureStorage")))
                {
                    siloBuilder.AddAzureQueueStreams(
                        "EventStream",
                        configurator => configurator.Configure(options =>
                        {
                            options.ConnectionString = configuration.GetConnectionString("AzureStorage");
                        }));
                }
            });

            return services;
        }
    }
}