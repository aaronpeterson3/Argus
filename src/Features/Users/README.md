# Users Feature

## Overview
The Users feature manages user accounts, profiles, and user-related operations in the system.

## Key Components

### API Endpoints
- `POST /api/users` - Create new user
- `GET /api/users/profile` - Get user profile
- `PUT /api/users/profile` - Update user profile
- `POST /api/users/change-password` - Change password

### Domain Models
- `UserProfile` - User profile information
- `UserState` - Orleans state management

### Services
- `UserService` - User management operations
- `UserValidator` - User data validation

### Orleans Grains
- `UserGrain` - User state management

## State Management
```csharp
public class UserState
{
    public string Id { get; set; }
    public string Email { get; set; }
    public UserProfile Profile { get; set; }
    public List<string> TenantIds { get; set; }
    // ...
}
```

## Usage Example
```csharp
// Get user grain
var grain = _grainFactory.GetGrain<IUserGrain>(userId);

// Update profile
var result = await grain.UpdateProfileAsync(newProfile);
```