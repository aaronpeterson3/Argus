using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using UserManagement.Grains;
using UserManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Orleans
builder.Host.UseOrleans((context, siloBuilder) =>
{
    var orleansConfig = context.Configuration.GetSection("Orleans").Get<OrleansConfig>();
    
    siloBuilder
        .UseLocalhostClustering(
            orleansConfig.SiloPort,
            orleansConfig.GatewayPort)
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UserGrain).Assembly).WithReferences())
        .AddMemoryGrainStorage("PubSubStore")
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = orleansConfig.ClusterId;
            options.ServiceId = orleansConfig.ServiceId;
        });
});

// Configure DbContext
builder.Services.AddDbContext<UserDbContext>(options =>
{
    var dbConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>();
    options.UseNpgsql(dbConfig.ConnectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();