using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Argus.Infrastructure.Authorization.Handlers;

// ... rest of the file stays the same ...

// Update the authorization handler registration
builder.Services.AddScoped<IAuthorizationHandler, TenantAuthorizationHandler>();