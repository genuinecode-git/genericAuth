# GenericAuth - Testing Guide

This guide provides step-by-step instructions to test the multi-tenant authentication system.

## Prerequisites

1. .NET 9.0 SDK installed
2. SQL Server (LocalDB or full instance)
3. Postman or curl for API testing

## Step 1: Database Setup

The application will automatically create the database and seed it with an Auth Admin user on startup.

```bash
# Navigate to the API project
cd src/GenericAuth.API

# Run the application
dotnet run
```

On first run, you should see logs indicating:
- Database migrations applied
- Auth Admin user created with credentials:
  - **Email**: admin@genericauth.com
  - **Password**: Admin@123

## Step 2: Auth Admin Workflow

### 2.1 Login as Auth Admin

**Endpoint**: `POST /api/auth/login`

```json
{
  "email": "admin@genericauth.com",
  "password": "Admin@123"
}
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "email": "admin@genericauth.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2025-11-08T..."
  }
}
```

**Important**: Copy the `token` value - you'll need it for subsequent requests.

### 2.2 Create an Application

**Endpoint**: `POST /api/applications`

**Headers**:
```
Authorization: Bearer {token_from_login}
```

**Body**:
```json
{
  "name": "HR Management System",
  "code": "HR_SYSTEM",
  "initialRoles": [
    {
      "name": "Admin",
      "description": "HR System Administrator",
      "isDefault": false
    },
    {
      "name": "Employee",
      "description": "Regular employee",
      "isDefault": true
    },
    {
      "name": "Manager",
      "description": "Department manager",
      "isDefault": false
    }
  ]
}
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "applicationId": "...",
    "code": "HR_SYSTEM",
    "apiKey": "base64-encoded-api-key-here",
    "message": "Application created successfully. Store the API key securely - it will not be shown again!"
  },
  "warning": "⚠️ IMPORTANT: Store the API key securely - it will not be shown again!"
}
```

**CRITICAL**: Save the `apiKey` value immediately! This is the only time it will be shown.

### 2.3 Register a Regular User

**Endpoint**: `POST /api/auth/register`

**Body**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "password": "User@123"
}
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "email": "john.doe@company.com",
    "message": "User registered successfully"
  }
}
```

**Important**: Copy the `userId` for the next step.

### 2.4 Assign User to Application

**Endpoint**: `POST /api/user-applications`

**Headers**:
```
Authorization: Bearer {auth_admin_token}
```

**Body**:
```json
{
  "userId": "{user_id_from_registration}",
  "applicationCode": "HR_SYSTEM",
  "roleName": "Employee"
}
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "applicationId": "...",
    "applicationCode": "HR_SYSTEM",
    "roleName": "Employee",
    "message": "User assigned successfully"
  }
}
```

## Step 3: Application-Scoped User Authentication

### 3.1 Login as Regular User (with Application Context)

**Endpoint**: `POST /api/auth/login`

**Headers**:
```
X-Application-Code: HR_SYSTEM
X-Api-Key: {api_key_from_step_2.2}
```

**Body**:
```json
{
  "email": "john.doe@company.com",
  "password": "User@123"
}
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "email": "john.doe@company.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2025-11-08T...",
    "applicationId": "...",
    "applicationCode": "HR_SYSTEM",
    "applicationRole": "Employee"
  }
}
```

The returned JWT token will contain application-scoped claims:
- `application_id`: The application ID
- `application_code`: HR_SYSTEM
- `application_role`: Employee
- `user_type`: Regular

## Step 4: Verify JWT Token Claims

You can decode the JWT token at [jwt.io](https://jwt.io) to verify the claims:

### Auth Admin Token Claims:
```json
{
  "sub": "user_id",
  "email": "admin@genericauth.com",
  "user_type": "AuthAdmin",
  "jti": "...",
  "exp": ...,
  "iss": "GenericAuth",
  "aud": "GenericAuthClient"
}
```

### Application-Scoped User Token Claims:
```json
{
  "sub": "user_id",
  "email": "john.doe@company.com",
  "user_type": "Regular",
  "application_id": "...",
  "application_code": "HR_SYSTEM",
  "application_role": "Employee",
  "permission": ["Read:Users", "Update:Profile"],
  "jti": "...",
  "exp": ...,
  "iss": "GenericAuth",
  "aud": "GenericAuthClient"
}
```

## Step 5: Test Authorization Policies

### 5.1 Auth Admin Only Endpoints

These endpoints require `user_type: AuthAdmin` claim:
- `POST /api/applications` - Create application
- `GET /api/applications/by-code/{code}` - Get application
- `POST /api/user-applications` - Assign user to application

Try accessing these with a regular user token - you should get `403 Forbidden`.

### 5.2 Application-Scoped Endpoints

These endpoints require `application_id` claim (regular users only):
- Any endpoint with `[Authorize(Policy = "RequireApplication")]`

## Step 6: Test Multiple Applications

Create a second application and assign the same user to it with a different role:

1. Create "Finance System" application with roles: Admin, Accountant, Viewer
2. Assign John Doe to Finance System with "Accountant" role
3. Login with `X-Application-Code: FINANCE_SYSTEM` header
4. Verify the token contains different `application_code` and `application_role`

## Common Issues

### Issue: "Application not found"
- Verify `X-Application-Code` header is correct (uppercase)
- Ensure the application exists in the database

### Issue: "Invalid API key"
- Verify `X-Api-Key` header contains the exact API key from application creation
- API keys are hashed - you cannot retrieve them again

### Issue: "User not assigned to application"
- Use Auth Admin token to assign the user via `POST /api/user-applications`
- Verify the user ID and application code are correct

### Issue: "Unauthorized"
- Check if the JWT token is expired
- Verify the token is included in the `Authorization: Bearer {token}` header
- Ensure the user has the required claims for the endpoint

## Database Inspection

To inspect the database:

```sql
-- View all applications
SELECT * FROM Applications;

-- View application roles
SELECT * FROM ApplicationRoles;

-- View user-application assignments
SELECT * FROM UserApplications;

-- View all users
SELECT * FROM Users;
```

## Next Steps

1. Implement additional endpoints (see TODOs in controllers)
2. Add permission-based authorization
3. Implement application activation/deactivation
4. Add API key rotation functionality
5. Implement role changing for users
6. Add application-scoped refresh token flow

## Security Notes

- The default Auth Admin password (`Admin@123`) should be changed immediately in production
- API keys are shown only once - store them securely
- JWT tokens expire after 60 minutes (configurable in appsettings.json)
- All passwords are hashed using PBKDF2 with SHA-256
- API keys are hashed using SHA-256
