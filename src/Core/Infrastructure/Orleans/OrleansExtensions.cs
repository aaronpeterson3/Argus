using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Argus.Core.Infrastructure.Orleans
{
    public static class OrleansExtensions
    {
        public static IServiceCollection AddOrleansConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOrleans(siloBuilder =>
            {
                if (configuration.GetValue<bool>("UseAzureStorage"))
                {
                    // Azure Storage configuration
                    siloBuilder.UseAzureStorageClustering(options =>
                        options.ConnectionString = configuration.GetConnectionString("AzureStorage"));

                    siloBuilder.AddAzureTableGrainStorage(
                        "tenant-store",
                        options => options.ConnectionString = configuration.GetConnectionString("AzureStorage"));

                    siloBuilder.AddAzureTableGrainStorage(
                        "user-store",
                        options => options.ConnectionString = configuration.GetConnectionString("AzureStorage"));

                    siloBuilder.AddAzureTableGrainStorage(
                        "job-store",
                        options => options.ConnectionString = configuration.GetConnectionString("AzureStorage"));
                }
                else
                {
                    // Development configuration
                    siloBuilder.UseLocalhostClustering();
                    siloBuilder.AddMemoryGrainStorage("tenant-store");
                    siloBuilder.AddMemoryGrainStorage("user-store");
                    siloBuilder.AddMemoryGrainStorage("job-store");
                }

                // Configure grain collection
                siloBuilder.Configure<GrainCollectionOptions>(options =>
                {
                    options.CollectionAge = TimeSpan.FromMinutes(1);
                    options.CollectionQuantum = TimeSpan.FromSeconds(30);
                });

                // Add reminder service
                if (configuration.GetValue<bool>("UseAzureStorage"))
                {
                    siloBuilder.UseAzureTableReminderService(options =>
                        options.ConnectionString = configuration.GetConnectionString("AzureStorage"));
                }
                else
                {
                    siloBuilder.UseInMemoryReminderService();
                }

                // Add grain type filters if needed
                siloBuilder.Configure<GrainTypeOptions>(options =>
                {
                    options.Filters.Add("Argus.Features.*");
                });
            });

            return services;
        }
    }
}