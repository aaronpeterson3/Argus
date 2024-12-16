# Argus - Multi-tenant Platform

Argus is a modern multi-tenant application platform built with ASP.NET Core and React, using Microsoft Orleans for distributed computing.

## Features

- Multi-tenant architecture
- User management with invitation system
- Role-based authorization
- Distributed state management with Orleans
- React-based web interface
- REST API with Swagger documentation

## Project Structure

```
Argus/
├── src/
│   ├── Core/                       # Core shared functionality
│   │   ├── Common/                 # Shared utilities
│   │   ├── Infrastructure/         # Cross-cutting concerns
│   │   └── Orleans/                # Orleans configuration
│   │
│   ├── Features/                   # Feature modules
│   │   ├── Authentication/         # Authentication feature
│   │   ├── Users/                  # User management
│   │   └── Tenants/               # Tenant management
│   │
│   ├── Argus.Api/                 # API host application
│   ├── Argus.Web/                 # React frontend
│   ├── Argus.Grains/              # Orleans grains
│   ├── Argus.Abstractions/        # Shared interfaces
│   ├── Argus.Infrastructure/      # Infrastructure services
│   └── Argus.Tests/               # Test projects
│
└── db/                            # Database migrations
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker Desktop
- SQL Server (or SQL Server container)

### Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/aaronpeterson3/Argus.git
   ```

2. Start infrastructure services
   ```bash
   docker-compose up -d
   ```

3. Run database migrations
   ```bash
   dotnet ef database update
   ```

4. Start the API
   ```bash
   cd src/Argus.Api
   dotnet run
   ```

5. Start the frontend
   ```bash
   cd src/Argus.Web
   npm install
   npm run dev
   ```

### First-time Setup

1. Navigate to `/system/init` to create the first tenant
2. Create an admin user for the tenant
3. Use the admin account to manage users and tenants

## Technology Stack

### Backend
- ASP.NET Core 8
- Microsoft Orleans
- Entity Framework Core
- PostgreSQL
- JWT Authentication

### Frontend
- React 18
- TypeScript
- TailwindCSS
- React Query
- React Router
- Formik & Yup

## Architecture

- Vertical slice architecture for features
- CQRS pattern using MediatR
- Orleans for distributed state management
- Multi-tenant data isolation
- RESTful API design

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
