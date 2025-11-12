# Getting Started with GenericAuth

Welcome to GenericAuth! This guide will help you get up and running quickly with the multi-tenant authentication API.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Prerequisites](#prerequisites)
3. [Installation & Setup](#installation--setup)
4. [First Steps](#first-steps)
5. [Common Workflows](#common-workflows)
6. [Development Setup](#development-setup)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## Quick Start

Get GenericAuth running in 5 minutes:

```bash
# 1. Clone the repository
git clone <repository-url>
cd genericAuth

# 2. Restore dependencies
dotnet restore

# 3. Update database connection string (optional)
# Edit src/GenericAuth.API/appsettings.json

# 4. Run the application
dotnet run --project src/GenericAuth.API/GenericAuth.API.csproj

# 5. Access Swagger UI
# Open http://localhost:5000 in your browser
```

That's it! The API is now running with seeded data.

---

## Prerequisites

### Required

- **.NET 8.0 SDK** or later
  - Download: https://dotnet.microsoft.com/download
  - Verify: `dotnet --version`

- **SQL Server** (any edition)
  - LocalDB (included with Visual Studio)
  - SQL Server Express (free)
  - SQL Server Developer Edition (free)
  - Azure SQL Database

### Recommended Tools

- **Visual Studio 2022** or **Visual Studio Code**
- **Postman** or similar API testing tool
- **Git** for version control
- **SQL Server Management Studio (SSMS)** for database management

---

## Installation & Setup

### Step 1: Clone the Repository

```bash
git clone <repository-url>
cd genericAuth
```

### Step 2: Configure Database Connection

Edit `src/GenericAuth.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GenericAuthDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Connection String Examples:**

**LocalDB (Default):**
```
Server=(localdb)\\mssqllocaldb;Database=GenericAuthDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

**SQL Server Express:**
```
Server=localhost\\SQLEXPRESS;Database=GenericAuthDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

**SQL Server with Authentication:**
```
Server=localhost;Database=GenericAuthDb;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true;TrustServerCertificate=true
```

**Azure SQL Database:**
```
Server=tcp:your-server.database.windows.net,1433;Database=GenericAuthDb;User ID=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Step 3: Configure JWT Settings (Optional)

The default JWT settings work for development. For production, update:

```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "GenericAuth",
    "Audience": "GenericAuthUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Important:** Change the `Secret` for production!

### Step 4: Restore Dependencies

```bash
dotnet restore
```

### Step 5: Build the Solution

```bash
dotnet build
```

### Step 6: Run Database Migrations

Migrations run automatically on startup, but you can run them manually:

```bash
cd src/GenericAuth.API
dotnet ef database update --project ../GenericAuth.Infrastructure/GenericAuth.Infrastructure.csproj
```

### Step 7: Run the Application

```bash
dotnet run --project src/GenericAuth.API/GenericAuth.API.csproj
```

The API will start on:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001 (if configured)

### Step 8: Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5000
```

You should see the Swagger UI with all API endpoints documented and ready to test.

---

## First Steps

### Understanding the Seeded Data

When you first run the application, it automatically seeds the database with:

**1. Auth Admin User:**
- Email: `admin@genericauth.com`
- Password: `Admin@123`
- Type: Auth Admin
- Can manage all applications, users, and roles

**2. System Roles:**
- SuperAdmin: Full system access
- Admin: Administrative access
- Support: Support team access

**3. Sample Permissions:**
- Various system permissions for Auth Admins
- Application permissions for regular users

### Your First API Calls

#### Step 1: Login as Auth Admin

Open Swagger UI and navigate to **POST /api/v1/auth/login**

Click "Try it out" and use:
```json
{
  "email": "admin@genericauth.com",
  "password": "Admin@123"
}
```

Click "Execute" and you'll receive:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "user": {
      "id": "...",
      "email": "admin@genericauth.com",
      "firstName": "System",
      "lastName": "Admin",
      "userType": "AuthAdmin"
    }
  }
}
```

**Copy the `accessToken` value.**

#### Step 2: Authorize in Swagger

At the top of the Swagger UI, click the **"Authorize"** button.

Enter:
```
Bearer your_access_token_here
```

Click "Authorize" and then "Close".

Now you can call all protected endpoints!

#### Step 3: Create Your First Application

Navigate to **POST /api/v1/applications** and execute:

```json
{
  "name": "My First App",
  "code": "MY_FIRST_APP",
  "initialRoles": [
    {
      "name": "Admin",
      "description": "Application administrator",
      "isDefault": false
    },
    {
      "name": "User",
      "description": "Standard user",
      "isDefault": true
    },
    {
      "name": "Guest",
      "description": "Guest access",
      "isDefault": false
    }
  ]
}
```

Response:
```json
{
  "success": true,
  "data": {
    "applicationId": "app-guid-here",
    "code": "MY_FIRST_APP",
    "apiKey": "sk_live_abc123...",
    "message": "Application created successfully with 3 roles."
  },
  "warning": "⚠️ IMPORTANT: Store the API key securely - it will not be shown again!"
}
```

**IMPORTANT:** Save the API key! You'll need it for application authentication.

#### Step 4: Create a Regular User

Navigate to **POST /api/v1/auth/register** and execute:

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "User@123456"
}
```

This creates a Regular user (not Auth Admin).

#### Step 5: Assign User to Application

Navigate to **POST /api/v1/user-applications** and execute:

```json
{
  "userId": "user-guid-from-step-4",
  "applicationCode": "MY_FIRST_APP",
  "roleName": "User"
}
```

If you omit `roleName`, the default role ("User") will be assigned.

#### Step 6: Login as Regular User

Navigate to **POST /api/v1/auth/login** and execute:

```json
{
  "email": "john.doe@example.com",
  "password": "User@123456",
  "applicationId": "app-guid-from-step-3"
}
```

**Note:** Regular users MUST provide an `applicationId` when logging in.

The response includes:
```json
{
  "success": true,
  "data": {
    "accessToken": "...",
    "refreshToken": "...",
    "user": {
      "email": "john.doe@example.com",
      "userType": "Regular"
    }
  }
}
```

The access token contains application-specific claims:
- `application_id`: The application GUID
- `application_code`: "MY_FIRST_APP"
- `application_role`: "User"

---

## Common Workflows

### Workflow 1: Setting Up a New Application

**Scenario:** You want to create a new tenant application called "E-Commerce Platform".

**Steps:**

1. **Create Application** (Auth Admin)
```bash
POST /api/v1/applications
{
  "name": "E-Commerce Platform",
  "code": "ECOMMERCE",
  "initialRoles": [
    {
      "name": "Store Owner",
      "description": "Store owner with full access",
      "isDefault": false
    },
    {
      "name": "Manager",
      "description": "Store manager",
      "isDefault": false
    },
    {
      "name": "Seller",
      "description": "Product seller",
      "isDefault": true
    },
    {
      "name": "Customer",
      "description": "Customer account",
      "isDefault": false
    }
  ]
}
```

2. **Add More Roles (if needed)** (Auth Admin)
```bash
POST /api/v1/applications/{appId}/roles
{
  "name": "Support Agent",
  "description": "Customer support",
  "isDefault": false
}
```

3. **Configure Permissions** (Auth Admin)
```bash
# Add permission to role
POST /api/v1/applications/{appId}/roles/{roleId}/permissions/{permissionId}
```

4. **Onboard Users** (Auth Admin)
```bash
# Register user
POST /api/v1/auth/register
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@store.com",
  "password": "Secure@123"
}

# Assign to application
POST /api/v1/user-applications
{
  "userId": "{userId}",
  "applicationCode": "ECOMMERCE",
  "roleName": "Store Owner"
}
```

5. **User Logs In**
```bash
POST /api/v1/auth/login
{
  "email": "jane@store.com",
  "password": "Secure@123",
  "applicationId": "{ecommerceAppId}"
}
```

---

### Workflow 2: Managing User Access

**Scenario:** A user needs to be promoted from "User" to "Admin" in an application.

**Steps:**

1. **Check Current Role**
```bash
GET /api/v1/user-applications/users/{userId}/applications
```

2. **Change Role**
```bash
PUT /api/v1/user-applications/users/{userId}/applications/{appCode}/role
{
  "newApplicationRoleId": "{adminRoleId}"
}
```

3. **Verify Change**
```bash
GET /api/v1/users/{userId}
```

**Note:** User must logout and login again for new role to take effect in JWT token.

---

### Workflow 3: Password Reset Flow

**Scenario:** User forgot their password.

**Steps:**

1. **User Requests Reset**
```bash
POST /api/v1/auth/forgot-password
{
  "email": "user@example.com"
}
```

2. **System Generates Token**
   - Token is stored in database
   - In production, email would be sent to user
   - For development, retrieve token from database

3. **User Resets Password**
```bash
POST /api/v1/auth/reset-password
{
  "email": "user@example.com",
  "resetToken": "abc123def456",
  "newPassword": "NewSecure@123"
}
```

4. **User Logs In with New Password**
```bash
POST /api/v1/auth/login
{
  "email": "user@example.com",
  "password": "NewSecure@123",
  "applicationId": "{appId}"
}
```

---

### Workflow 4: Token Refresh Flow

**Scenario:** Access token expired, need to refresh without re-login.

**Steps:**

1. **API Returns 401 Unauthorized**
   - Access token has expired (15 minutes default)

2. **Client Refreshes Token**
```bash
POST /api/v1/auth/refresh
{
  "refreshToken": "your_refresh_token"
}
```

3. **Receive New Tokens**
```json
{
  "success": true,
  "data": {
    "accessToken": "new_access_token",
    "refreshToken": "new_refresh_token",
    "expiresIn": 900
  }
}
```

4. **Update Stored Tokens**
   - Store new access token
   - Store new refresh token (old one is revoked)
   - Retry original request with new access token

**Implementation Example (JavaScript):**
```javascript
async function apiCall(url, options) {
  let response = await fetch(url, options);

  if (response.status === 401) {
    // Token expired, try to refresh
    const refreshed = await refreshToken();
    if (refreshed) {
      // Retry with new token
      options.headers['Authorization'] = `Bearer ${getAccessToken()}`;
      response = await fetch(url, options);
    } else {
      // Refresh failed, redirect to login
      redirectToLogin();
    }
  }

  return response;
}

async function refreshToken() {
  const response = await fetch('/api/v1/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: getRefreshToken() })
  });

  if (response.ok) {
    const data = await response.json();
    storeAccessToken(data.data.accessToken);
    storeRefreshToken(data.data.refreshToken);
    return true;
  }

  return false;
}
```

---

### Workflow 5: Multi-Application User Access

**Scenario:** A user needs access to multiple applications with different roles.

**Steps:**

1. **Assign User to First Application**
```bash
POST /api/v1/user-applications
{
  "userId": "{userId}",
  "applicationCode": "APP_ONE",
  "roleName": "Admin"
}
```

2. **Assign User to Second Application**
```bash
POST /api/v1/user-applications
{
  "userId": "{userId}",
  "applicationCode": "APP_TWO",
  "roleName": "User"
}
```

3. **User Logs Into First Application**
```bash
POST /api/v1/auth/login
{
  "email": "user@example.com",
  "password": "password",
  "applicationId": "{appOneId}"
}
```
Token contains: `application_role: "Admin"`

4. **User Switches to Second Application**
```bash
POST /api/v1/auth/login
{
  "email": "user@example.com",
  "password": "password",
  "applicationId": "{appTwoId}"
}
```
Token contains: `application_role: "User"`

**Note:** User must login separately for each application to get application-specific tokens.

---

## Development Setup

### Running with Visual Studio

1. Open `GenericAuth.sln` in Visual Studio 2022
2. Set `GenericAuth.API` as startup project
3. Press F5 to run with debugging
4. Browser opens to Swagger UI automatically

### Running with Visual Studio Code

1. Open project folder in VS Code
2. Install recommended extensions:
   - C# (Microsoft)
   - C# Dev Kit
   - REST Client (for testing)
3. Press F5 to run with debugging
4. Select ".NET Core Launch (web)" configuration

### Running from Command Line

```bash
# Development mode (with hot reload)
cd src/GenericAuth.API
dotnet watch run

# Production mode
dotnet run --configuration Release
```

### Database Management

**View Migrations:**
```bash
cd src/GenericAuth.Infrastructure
dotnet ef migrations list
```

**Add New Migration:**
```bash
cd src/GenericAuth.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../GenericAuth.API
```

**Update Database:**
```bash
cd src/GenericAuth.Infrastructure
dotnet ef database update --startup-project ../GenericAuth.API
```

**Reset Database:**
```bash
cd src/GenericAuth.Infrastructure
dotnet ef database drop --startup-project ../GenericAuth.API
dotnet ef database update --startup-project ../GenericAuth.API
```

### Configuration Environments

**Development (appsettings.Development.json):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Production (appsettings.Production.json):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Production connection string"
  },
  "JwtSettings": {
    "Secret": "Production secret from environment variables"
  }
}
```

**Using Environment Variables:**
```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=..."
export JwtSettings__Secret="YourProductionSecret"
dotnet run

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=..."
$env:JwtSettings__Secret="YourProductionSecret"
dotnet run

# Windows CMD
set ConnectionStrings__DefaultConnection=Server=...
set JwtSettings__Secret=YourProductionSecret
dotnet run
```

---

## Testing

### Running Unit Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=opencover

# Run specific test project
dotnet test tests/GenericAuth.Application.UnitTests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Running Integration Tests

```bash
# Run integration tests
dotnet test tests/GenericAuth.API.IntegrationTests

# Run specific test
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Using Postman

1. Import the Postman collection: `docs/GenericAuth.postman_collection.json`
2. Set environment variables:
   - `base_url`: http://localhost:5000
   - `api_version`: v1
3. Run "Login as Auth Admin" request
4. Token is automatically saved to `auth_token` variable
5. All subsequent requests use this token

### Manual Testing with cURL

```bash
# Register user
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com",
    "password": "Test@123456"
  }'

# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@genericauth.com",
    "password": "Admin@123"
  }'

# Save token to variable (Linux/Mac)
TOKEN=$(curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@genericauth.com","password":"Admin@123"}' \
  | jq -r '.data.accessToken')

# Use token in subsequent requests
curl -X GET http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $TOKEN"
```

---

## Troubleshooting

### Issue: Database Connection Failed

**Error:** "A network-related or instance-specific error occurred..."

**Solutions:**
1. Verify SQL Server is running
2. Check connection string in appsettings.json
3. Test connection with SSMS
4. For LocalDB: `sqllocaldb start mssqllocaldb`
5. Enable TCP/IP in SQL Server Configuration Manager

### Issue: Migration Failed

**Error:** "Unable to create migration..."

**Solutions:**
1. Ensure Infrastructure project is in correct directory
2. Run from Infrastructure directory with --startup-project flag
3. Check DbContext registration in Infrastructure layer
4. Verify connection string is accessible

### Issue: JWT Token Invalid

**Error:** 401 Unauthorized

**Solutions:**
1. Verify token is not expired (15 min default)
2. Check token is in format: `Bearer {token}`
3. Ensure no extra spaces in Authorization header
4. Verify JWT secret matches in appsettings.json
5. Use refresh token to get new access token

### Issue: Seeding Failed

**Error:** Database seeding errors on startup

**Solutions:**
1. Drop and recreate database
2. Check for duplicate data
3. Review seeder logs in console
4. Disable seeding temporarily: Comment out seeding code in Program.cs

### Issue: Swagger Not Loading

**Error:** 404 or blank page

**Solutions:**
1. Verify application is running on correct port
2. Check URL: http://localhost:5000 (not /swagger)
3. Ensure Development environment
4. Check Program.cs for Swagger configuration
5. Review browser console for JavaScript errors

### Issue: CORS Errors

**Error:** "No 'Access-Control-Allow-Origin' header"

**Solutions:**
1. Check CORS policy in Program.cs
2. Verify origin is allowed
3. Add specific origins instead of AllowAny
4. Check request includes credentials

### Issue: Validation Errors

**Error:** 400 Bad Request with validation errors

**Solutions:**
1. Read error messages carefully (they're descriptive)
2. Check required fields
3. Verify data types and formats
4. Review FluentValidation rules
5. Check string lengths

### Issue: Role Assignment Failed

**Error:** "System roles can only be assigned to Auth Admin users"

**Solutions:**
1. Verify user type (Regular vs AuthAdmin)
2. For Regular users, use Application Roles
3. For Auth Admins, use System Roles
4. Check endpoint documentation

---

## Next Steps

Now that you're up and running:

1. **Read the API Reference** - Comprehensive endpoint documentation
   - See: `docs/API_REFERENCE.md`

2. **Understand the Architecture** - Learn about Clean Architecture and DDD
   - See: `docs/ARCHITECTURE_GUIDE.md`

3. **Explore the Code** - Study the implementation
   - Domain Layer: Business logic and entities
   - Application Layer: CQRS commands and queries
   - Infrastructure Layer: Database and external services
   - API Layer: Controllers and middleware

4. **Customize for Your Needs**
   - Add custom permissions
   - Extend user profile
   - Add custom claims
   - Integrate with your application

5. **Deploy to Production**
   - Configure production database
   - Set up proper JWT secrets
   - Enable HTTPS
   - Configure logging
   - Set up monitoring

---

## Getting Help

- **API Reference**: Complete endpoint documentation
- **Architecture Guide**: System design and patterns
- **Integration Tests**: See `tests/GenericAuth.API.IntegrationTests` for examples
- **Swagger UI**: Interactive API documentation at http://localhost:5000

---

**Happy Coding!**

Last Updated: November 9, 2025
