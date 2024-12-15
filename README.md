# Argus - Multi-tenant Application Platform

## Architecture Overview

Argus is built using vertical slice architecture, organizing code around business features rather than technical concerns. Each feature is self-contained with its own API endpoints, domain logic, and UI components.

### Key Features
- Multi-tenancy support
- User authentication and authorization
- Profile management
- Tenant management and user invitations

### Technology Stack
- ASP.NET Core 8.0
- Microsoft Orleans for distributed computing
- Blazor WebAssembly for the frontend
- SQL Server for relational data
- Azure Table Storage for Orleans persistence

## Project Structure

```
src/
├── Core/                           # Shared infrastructure
│   ├── Common/                     # Shared utilities
│   ├── Infrastructure/             # Cross-cutting concerns
│   └── Orleans/                    # Orleans configuration
├── Features/                       # Feature slices
│   ├── Authentication/             # Auth feature
│   ├── Users/                      # User management
│   └── Tenants/                    # Tenant management
├── Web/                           # Web host
└── Api/                           # API host
```

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop
- SQL Server (or SQL Server container)
- Visual Studio 2022 or VS Code

### Local Development
1. Clone the repository
   ```bash
   git clone https://github.com/aaronpeterson3/Argus.git
   ```

2. Start the development infrastructure
   ```bash
   docker-compose up -d
   ```

3. Run the migrations
   ```bash
   dotnet ef database update
   ```

4. Start the application
   ```bash
   dotnet run --project src/Api/Api.csproj
   dotnet run --project src/Web/Web.csproj
   ```

### Running Tests
```bash
dotnet test
```

## Feature Documentation
- [Authentication](src/Features/Authentication/README.md)
- [Users](src/Features/Users/README.md)
- [Tenants](src/Features/Tenants/README.md)

## Architecture Decisions

### Vertical Slice Architecture
The application uses vertical slice architecture to:
- Organize code around business features
- Maintain high cohesion within features
- Reduce coupling between features
- Enable independent feature evolution

### CQRS Pattern
- Commands and queries are separated
- MediatR for in-process command/query handling
- Validation and logging behaviors in the pipeline

### Multi-tenancy
- Hybrid multi-tenant approach
- Tenant isolation at the data level
- Middleware for tenant context

### Orleans Integration
- Distributed state management
- Actor-based concurrency model
- Scalable and resilient architecture

## Contributing
Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details