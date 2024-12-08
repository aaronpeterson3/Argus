var builder = WebApplication.CreateBuilder(args);

// ... previous configuration ...

// Configure Infrastructure
builder.Services.AddInfrastructure();

// ... rest of Program.cs content ...