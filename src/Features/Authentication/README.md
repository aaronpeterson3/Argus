# Authentication Feature

## Overview
The Authentication feature handles all user authentication and authorization concerns, including login, password management, and token handling.

## Key Components

### API Endpoints
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/forgot-password` - Initiate password reset
- `POST /api/auth/reset-password` - Complete password reset

### Services
- `AuthenticationService` - Core authentication logic
- `TokenService` - JWT token management
- `EmailService` - Authentication-related notifications

### Web Components
- `Login.razor` - Login page
- `ForgotPassword.razor` - Password reset workflow

## Configuration
```json
{
  "Auth": {
    "TokenExpirationMinutes": 60,
    "ResetTokenExpirationHours": 24,
    "RequireConfirmedAccount": true
  }
}
```

## Usage Example
```csharp
// Inject the service
@inject IAuthenticationService AuthService

// Login
var result = await AuthService.LoginAsync(email, password);
if (result.IsSuccess)
{
    // Handle successful login
}
```