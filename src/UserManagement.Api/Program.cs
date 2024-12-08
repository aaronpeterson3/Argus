using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using UserManagement.Api.Services;
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

// Configure JWT Authentication
var authConfig = builder.Configuration.GetSection("Authentication").Get<AuthConfig>();
builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authConfig.JwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Services
builder.Services.AddScoped<JwtTokenService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();