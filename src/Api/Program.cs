using Argus.Core.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Argus API", Version = "v1" });
});

// Add Orleans
builder.Host.UseOrleans(siloBuilder =>
{
    // Orleans configuration will be moved here
});

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    // Register features assemblies
    cfg.RegisterServicesFromAssemblyContaining(typeof(Argus.Features.Authentication.Api.AuthenticationController));
    cfg.RegisterServicesFromAssemblyContaining(typeof(Argus.Features.Users.Api.UsersController));
    cfg.RegisterServicesFromAssemblyContaining(typeof(Argus.Features.Tenants.Api.TenantsController));
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