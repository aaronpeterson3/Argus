# Tenants Feature

## Overview
The Tenants feature handles multi-tenancy support, tenant management, and user-tenant relationships.

## Key Components

### API Endpoints
- `POST /api/tenants` - Create tenant
- `GET /api/tenants/{id}` - Get tenant details
- `PUT /api/tenants/{id}` - Update tenant
- `POST /api/tenants/{id}/users/invite` - Invite user

### Domain Models
- `TenantState` - Tenant information and settings
- `TenantUserInfo` - User-tenant relationship
- `TenantInvite` - Invitation tracking

### Services
- `TenantService` - Tenant management operations
- `TenantValidator` - Tenant data validation

### Orleans Grains
- `TenantGrain` - Tenant state management

## Multi-tenancy
The system uses a hybrid multi-tenancy approach:
- Shared application with tenant isolation
- Per-tenant data separation in Orleans grains
- Tenant context middleware for request isolation

## Usage Example
```csharp
// Create new tenant
var tenantGrain = _grainFactory.GetGrain<ITenantGrain>(tenantId);
var result = await tenantGrain.CreateAsync("New Tenant", ownerId);

// Invite user
var inviteResult = await tenantGrain.InviteUserAsync("user@example.com", "User");
```