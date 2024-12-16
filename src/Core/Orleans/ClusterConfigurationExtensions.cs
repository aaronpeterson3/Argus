using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Argus.Core.Orleans
{
    public static class ClusterConfigurationExtensions
    {
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, IConfiguration configuration)
        {
            var orleansConfig = configuration.GetSection("Orleans").Get<OrleansConfig>();
            var redisConfig = configuration.GetSection("Redis").Get<RedisConfig>();

            builder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansConfig.ClusterId;
                options.ServiceId = orleansConfig.ServiceId;
            });

            if (configuration.GetValue<bool>("UseRedisStorage"))
            {
                // Redis clustering and storage
                builder.UseRedisClustering(options =>
                {
                    options.ConnectionString = redisConfig.ConnectionString;
                    options.Database = 0;
                });

                builder.AddRedisGrainStorage("tenant-store", options =>
                {
                    options.ConnectionString = redisConfig.ConnectionString;
                    options.Database = 1;
                });

                builder.AddRedisGrainStorage("user-store", options =>
                {
                    options.ConnectionString = redisConfig.ConnectionString;
                    options.Database = 2;
                });
            }
            else
            {
                // In-memory for development
                builder.UseInMemoryReminderService()
                    .AddMemoryGrainStorage("tenant-store")
                    .AddMemoryGrainStorage("user-store");
            }

            builder.UseTransactions();

            return builder;
        }
    }

    public class OrleansConfig
    {
        public string ClusterId { get; set; } = "dev";
        public string ServiceId { get; set; } = "ArgusService";
        public int SiloPort { get; set; } = 11111;
        public int GatewayPort { get; set; } = 30000;
    }

    public class RedisConfig
    {
        public string ConnectionString { get; set; }
    }
}