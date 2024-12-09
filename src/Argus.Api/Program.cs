using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Configuration;

// ... rest of the using statements ...

builder.Host.UseOrleans((context, siloBuilder) =>
{
    var orleansConfig = context.Configuration.GetSection("Orleans").Get<OrleansConfig>();
    
    siloBuilder
        .UseLocalhostClustering(
            orleansConfig.SiloPort,
            orleansConfig.GatewayPort)
        .ConfigureServices(services =>
        {
            services.AddApplicationPart(typeof(UserGrain).Assembly).WithReferences();
        })
        .AddMemoryGrainStorage("PubSubStore")
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = orleansConfig.ClusterId;
            options.ServiceId = orleansConfig.ServiceId;
        });
});