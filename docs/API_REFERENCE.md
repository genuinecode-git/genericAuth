# GenericAuth API Reference

Version: 1.0
Base URL: `http://localhost:5000/api/v1`

## Table of Contents

1. [Introduction](#introduction)
2. [Authentication & Authorization](#authentication--authorization)
3. [API Endpoints](#api-endpoints)
   - [Authentication](#authentication-endpoints)
   - [Users](#users-endpoints)
   - [Applications](#applications-endpoints)
   - [Application Roles](#application-roles-endpoints)
   - [System Roles](#system-roles-endpoints)
   - [User-Application Assignments](#user-application-assignments-endpoints)
4. [Data Models](#data-models)
5. [Error Handling](#error-handling)

---

## Introduction

GenericAuth is a comprehensive multi-tenant authentication and authorization API built with **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** using MediatR. It provides enterprise-grade authentication with JWT tokens and supports application-scoped role-based access control.

### Key Features

- **Multi-Tenant Architecture**: Application-scoped roles for complete tenant isolation
- **Dual User Types**: Auth Admins (system administrators) and Regular Users (application users)
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **Role-Based Access Control**: Granular permissions for both system and application roles
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Command Query Responsibility Segregation using MediatR

### Multi-Tenant Architecture

GenericAuth implements multi-tenancy through **application-scoped roles**:

- **Applications**: Independent tenants with isolated role systems
- **Application Roles**: Roles specific to each application (e.g., "Admin", "User", "Viewer")
- **System Roles**: Administrative roles for Auth Admin users only
- **User-Application Assignments**: Users can belong to multiple applications with different roles

### User Types

**1. Auth Admin**
- System-level administrator
- Can manage applications, users, roles, and assignments
- Has access to system roles
- Identified by `user_type: "AuthAdmin"` claim in JWT

**2. Regular User**
- Application-level user
- Must be assigned to at least one application
- Can only access endpoints with valid application context
- Token includes `application_id` and `application_role` claims

---

## Authentication & Authorization

### Authentication Schemes

#### 1. Bearer Token (JWT)

All authenticated endpoints require a JWT token in the Authorization header:

```http
Authorization: Bearer <your_jwt_token>
```

#### 2. Application Authentication

Some endpoints (like login) can optionally use application-specific authentication with headers:

```http
X-Application-Code: your_app_code
X-Api-Key: your_api_key
```

### JWT Token Structure

**Auth Admin Token Claims:**
```json
{
  "sub": "user_id",
  "email": "admin@example.com",
  "user_type": "AuthAdmin",
  "role": "SuperAdmin",
  "jti": "token_id",
  "iat": "issued_at",
  "exp": "expiration_time"
}
```

**Regular User Token Claims:**
```json
{
  "sub": "user_id",
  "email": "user@example.com",
  "user_type": "Regular",
  "application_id": "app_guid",
  "application_code": "APP_CODE",
  "application_role": "Admin",
  "jti": "token_id",
  "iat": "issued_at",
  "exp": "expiration_time"
}
```

### Token Lifecycle

1. **Login**: Returns `accessToken` (15 min) and `refreshToken` (7 days)
2. **Use**: Include `accessToken` in Authorization header
3. **Refresh**: When `accessToken` expires, use `refreshToken` to get new tokens
4. **Logout**: Revoke refresh tokens (single or all)

### Authorization Policies

| Policy | Requirement | Usage |
|--------|-------------|-------|
| `AuthAdminOnly` | User must have `user_type: "AuthAdmin"` claim | All management endpoints |
| `RequireApplication` | User must have `application_id` claim | Application-specific endpoints |
| `RequireApplicationAdmin` | User must have `application_role: "Admin"` claim | Admin-level operations |

---

## API Endpoints

### Authentication Endpoints

#### POST /api/v1/auth/register

Register a new user account (creates Regular user by default).

**Authorization:** None (Public)

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "userType": "Regular",
    "isActive": true,
    "createdAt": "2025-11-09T18:30:00Z"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": [
    "Email already exists.",
    "Password must be at least 8 characters long."
  ]
}
```

**Validation Rules:**
- First Name: Required, 1-50 characters
- Last Name: Required, 1-50 characters
- Email: Required, valid email format, unique
- Password: Required, 8-100 characters, must contain uppercase, lowercase, digit, and special character

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "SecureP@ssw0rd123"
  }'
```

---

#### POST /api/v1/auth/login

Authenticate a user and receive JWT tokens.

**Authorization:** None (Public)

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd123",
  "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Fields:**
- `email` (required): User's email address
- `password` (required): User's password
- `applicationId` (optional): Required for Regular users, must be null for Auth Admins

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "userType": "Regular"
    }
  }
}
```

**Error Responses:**

*400 Bad Request - Invalid Credentials:*
```json
{
  "errors": ["Invalid email or password."]
}
```

*400 Bad Request - Inactive Account:*
```json
{
  "errors": ["User account is inactive."]
}
```

*400 Bad Request - Application Required:*
```json
{
  "errors": ["Regular users must provide an application ID."]
}
```

**cURL Examples:**

*Login as Auth Admin:*
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@genericauth.com",
    "password": "Admin@123"
  }'
```

*Login as Regular User:*
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "User@123",
    "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

---

#### POST /api/v1/auth/refresh

Refresh an expired access token using a refresh token.

**Authorization:** None (Public)

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900,
    "tokenType": "Bearer"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": ["Invalid or expired refresh token."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your_refresh_token_here"
  }'
```

**Notes:**
- Refresh tokens are single-use (rotation)
- A new refresh token is issued with each refresh
- Old refresh token is automatically revoked

---

#### POST /api/v1/auth/logout

Logout and revoke refresh tokens.

**Authorization:** Bearer Token (Required)

**Request Body (Optional):**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Behavior:**
- If `refreshToken` is provided: Revokes that specific token
- If `refreshToken` is null/omitted: Revokes ALL tokens for the user

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Logged out successfully."
}
```

**Error Response (401 Unauthorized):**
```json
{
  "errors": ["Invalid user token."]
}
```

**cURL Examples:**

*Logout specific session:*
```bash
curl -X POST http://localhost:5000/api/v1/auth/logout \
  -H "Authorization: Bearer your_access_token" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your_refresh_token"
  }'
```

*Logout all sessions:*
```bash
curl -X POST http://localhost:5000/api/v1/auth/logout \
  -H "Authorization: Bearer your_access_token" \
  -H "Content-Type: application/json" \
  -d '{}'
```

---

#### POST /api/v1/auth/forgot-password

Initiate password reset process.

**Authorization:** None (Public)

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "If the email exists, a password reset token has been generated."
}
```

**Notes:**
- Response is intentionally vague to prevent user enumeration
- Returns 200 OK even if email doesn't exist
- In production, should trigger email with reset token
- Token is stored in database with expiration (typically 1 hour)

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com"
  }'
```

---

#### POST /api/v1/auth/reset-password

Reset password using reset token.

**Authorization:** None (Public)

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "resetToken": "abc123def456",
  "newPassword": "NewSecureP@ssw0rd123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Password has been reset successfully."
}
```

**Error Responses:**

*400 Bad Request - Invalid Token:*
```json
{
  "errors": ["Invalid or expired reset token."]
}
```

*400 Bad Request - Password Validation:*
```json
{
  "errors": [
    "Password must be at least 8 characters long.",
    "Password must contain at least one uppercase letter."
  ]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "resetToken": "abc123def456",
    "newPassword": "NewSecureP@ssw0rd123"
  }'
```

---

### Users Endpoints

All user endpoints require **Auth Admin** authorization.

#### GET /api/v1/users

Get paginated list of users with search and filtering.

**Authorization:** Auth Admin Only

**Query Parameters:**
- `pageNumber` (optional, default: 1): Page number
- `pageSize` (optional, default: 10, max: 100): Items per page
- `searchTerm` (optional): Search in name or email
- `userType` (optional): Filter by "Regular" or "AuthAdmin"

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "userType": "Regular",
      "isActive": true,
      "createdAt": "2025-11-09T18:30:00Z",
      "updatedAt": "2025-11-09T18:30:00Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 45,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**cURL Examples:**

*Get all users (first page):*
```bash
curl -X GET "http://localhost:5000/api/v1/users?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer your_admin_token"
```

*Search users by email:*
```bash
curl -X GET "http://localhost:5000/api/v1/users?searchTerm=john&pageSize=20" \
  -H "Authorization: Bearer your_admin_token"
```

*Filter Auth Admin users:*
```bash
curl -X GET "http://localhost:5000/api/v1/users?userType=AuthAdmin" \
  -H "Authorization: Bearer your_admin_token"
```

---

#### GET /api/v1/users/{id}

Get detailed user information by ID.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): User GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "userType": "Regular",
    "isActive": true,
    "createdAt": "2025-11-09T18:30:00Z",
    "updatedAt": "2025-11-09T18:30:00Z",
    "applications": [
      {
        "applicationId": "app-guid-1",
        "applicationCode": "APP_ONE",
        "applicationName": "Application One",
        "roleId": "role-guid-1",
        "roleName": "Admin",
        "assignedAt": "2025-11-09T19:00:00Z"
      }
    ],
    "systemRoles": [
      {
        "roleId": "role-guid-2",
        "roleName": "SuperAdmin",
        "assignedAt": "2025-11-09T18:30:00Z"
      }
    ]
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["User not found."]
}
```

**cURL Example:**
```bash
curl -X GET http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer your_admin_token"
```

---

#### PUT /api/v1/users/{id}

Update user profile information.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): User GUID

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "john.doe@example.com",
    "userType": "Regular",
    "isActive": true,
    "updatedAt": "2025-11-09T20:00:00Z"
  }
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": [
    "First name is required.",
    "Last name must not exceed 50 characters."
  ]
}
```

**cURL Example:**
```bash
curl -X PUT http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith"
  }'
```

---

#### POST /api/v1/users/{id}/activate

Activate a user account.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): User GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "User activated successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["User is already active."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/activate \
  -H "Authorization: Bearer your_admin_token"
```

---

#### POST /api/v1/users/{id}/deactivate

Deactivate a user account.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): User GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "User deactivated successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["User is already inactive."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/deactivate \
  -H "Authorization: Bearer your_admin_token"
```

**Notes:**
- Deactivated users cannot login
- Existing sessions remain valid until token expiration
- All refresh tokens should be revoked (future enhancement)

---

#### POST /api/v1/users/{userId}/roles/{roleId}

Assign a system role to an Auth Admin user.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `userId` (required): User GUID
- `roleId` (required): System Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "System role assigned successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User not found."]
}
```

*400 Bad Request - Not Auth Admin:*
```json
{
  "errors": ["System roles can only be assigned to Auth Admin users."]
}
```

*400 Bad Request - Already Assigned:*
```json
{
  "errors": ["User already has this system role."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/users/user-guid/roles/role-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

#### DELETE /api/v1/users/{userId}/roles/{roleId}

Remove a system role from an Auth Admin user.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `userId` (required): User GUID
- `roleId` (required): System Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "System role removed successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["User does not have this system role."]
}
```

**cURL Example:**
```bash
curl -X DELETE http://localhost:5000/api/v1/users/user-guid/roles/role-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

### Applications Endpoints

All application endpoints require **Auth Admin** authorization.

#### POST /api/v1/applications

Create a new application with initial roles.

**Authorization:** Auth Admin Only

**Request Body:**
```json
{
  "name": "My Application",
  "code": "MY_APP",
  "initialRoles": [
    {
      "name": "Admin",
      "description": "Application administrator with full access",
      "isDefault": false
    },
    {
      "name": "User",
      "description": "Standard application user",
      "isDefault": true
    },
    {
      "name": "Viewer",
      "description": "Read-only access",
      "isDefault": false
    }
  ]
}
```

**Validation Rules:**
- `name`: Required, 1-100 characters
- `code`: Required, 2-50 characters, uppercase letters, numbers, underscores only, unique
- `initialRoles`: Required, at least one role, exactly one must be default

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "code": "MY_APP",
    "apiKey": "sk_live_abc123def456...",
    "message": "Application created successfully with 3 roles."
  },
  "warning": "⚠️ IMPORTANT: Store the API key securely - it will not be shown again!"
}
```

**Error Responses:**

*400 Bad Request - Duplicate Code:*
```json
{
  "errors": ["Application code 'MY_APP' already exists."]
}
```

*400 Bad Request - Validation:*
```json
{
  "errors": [
    "Application code can only contain uppercase letters, numbers, and underscores.",
    "Exactly one initial role must be set as default."
  ]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/applications \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Application",
    "code": "MY_APP",
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
      }
    ]
  }'
```

**Important Notes:**
- API key is shown ONLY once in this response
- Store the API key securely (environment variables, secrets manager)
- API key hash is stored in database for future authentication
- Cannot retrieve plain API key after creation

---

#### GET /api/v1/applications/by-code/{code}

Get application details by application code.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `code` (required): Application code (e.g., "MY_APP")

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "My Application",
    "code": "MY_APP",
    "isActive": true,
    "createdAt": "2025-11-09T18:30:00Z",
    "updatedAt": "2025-11-09T18:30:00Z",
    "defaultRoleId": "role-guid",
    "defaultRoleName": "User",
    "totalUsers": 42,
    "totalRoles": 3
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["Application not found."]
}
```

**cURL Example:**
```bash
curl -X GET http://localhost:5000/api/v1/applications/by-code/MY_APP \
  -H "Authorization: Bearer your_admin_token"
```

---

### Application Roles Endpoints

All application role endpoints require **Auth Admin** authorization.

#### GET /api/v1/applications/{appId}/roles

Get paginated list of roles for a specific application.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID

**Query Parameters:**
- `pageNumber` (optional, default: 1): Page number
- `pageSize` (optional, default: 10, max: 100): Items per page
- `searchTerm` (optional): Search in role name or description
- `isActive` (optional): Filter by active status (true/false)

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "role-guid-1",
      "applicationId": "app-guid",
      "name": "Admin",
      "description": "Application administrator with full access",
      "isDefault": false,
      "isActive": true,
      "userCount": 5,
      "permissionCount": 12,
      "createdAt": "2025-11-09T18:30:00Z"
    },
    {
      "id": "role-guid-2",
      "applicationId": "app-guid",
      "name": "User",
      "description": "Standard application user",
      "isDefault": true,
      "isActive": true,
      "userCount": 37,
      "permissionCount": 4,
      "createdAt": "2025-11-09T18:30:00Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalCount": 2,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["Application not found."]
}
```

**cURL Examples:**

*Get all roles for application:*
```bash
curl -X GET "http://localhost:5000/api/v1/applications/app-guid/roles" \
  -H "Authorization: Bearer your_admin_token"
```

*Search roles:*
```bash
curl -X GET "http://localhost:5000/api/v1/applications/app-guid/roles?searchTerm=admin&isActive=true" \
  -H "Authorization: Bearer your_admin_token"
```

---

#### GET /api/v1/applications/{appId}/roles/{roleId}

Get detailed information about a specific application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "role-guid",
    "applicationId": "app-guid",
    "applicationCode": "MY_APP",
    "name": "Admin",
    "description": "Application administrator with full access",
    "isDefault": false,
    "isActive": true,
    "userCount": 5,
    "createdAt": "2025-11-09T18:30:00Z",
    "updatedAt": "2025-11-09T18:30:00Z",
    "permissions": [
      {
        "id": "perm-guid-1",
        "name": "users.read",
        "description": "View users"
      },
      {
        "id": "perm-guid-2",
        "name": "users.write",
        "description": "Create and edit users"
      }
    ]
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["Application role not found."]
}
```

**cURL Example:**
```bash
curl -X GET http://localhost:5000/api/v1/applications/app-guid/roles/role-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

#### POST /api/v1/applications/{appId}/roles

Create a new role for an application.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID

**Request Body:**
```json
{
  "name": "Moderator",
  "description": "Content moderator with review permissions",
  "isDefault": false
}
```

**Validation Rules:**
- `name`: Required, 1-50 characters, unique within application
- `description`: Required, 1-200 characters
- `isDefault`: Optional, default false (only one role per app can be default)

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "new-role-guid",
    "applicationId": "app-guid",
    "name": "Moderator",
    "description": "Content moderator with review permissions",
    "isDefault": false,
    "isActive": true,
    "createdAt": "2025-11-09T20:00:00Z"
  }
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application not found."]
}
```

*400 Bad Request - Duplicate Name:*
```json
{
  "errors": ["A role with this name already exists for this application."]
}
```

*400 Bad Request - Default Conflict:*
```json
{
  "errors": ["Application already has a default role. Unset the existing default first."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/applications/app-guid/roles \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Moderator",
    "description": "Content moderator",
    "isDefault": false
  }'
```

---

#### PUT /api/v1/applications/{appId}/roles/{roleId}

Update an existing application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID

**Request Body:**
```json
{
  "name": "Senior Moderator",
  "description": "Senior content moderator with additional privileges"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "role-guid",
    "applicationId": "app-guid",
    "name": "Senior Moderator",
    "description": "Senior content moderator with additional privileges",
    "isDefault": false,
    "isActive": true,
    "updatedAt": "2025-11-09T20:30:00Z"
  }
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["A role with this name already exists for this application."]
}
```

**cURL Example:**
```bash
curl -X PUT http://localhost:5000/api/v1/applications/app-guid/roles/role-guid \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Senior Moderator",
    "description": "Senior content moderator"
  }'
```

**Notes:**
- Cannot change `isDefault` flag via update (use set-default endpoint)
- Name must be unique within application

---

#### DELETE /api/v1/applications/{appId}/roles/{roleId}

Delete an application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Application role deleted successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request - Default Role:*
```json
{
  "errors": ["Cannot delete the default role."]
}
```

*400 Bad Request - Has Users:*
```json
{
  "errors": ["Cannot delete role with assigned users. Reassign users first."]
}
```

**cURL Example:**
```bash
curl -X DELETE http://localhost:5000/api/v1/applications/app-guid/roles/role-guid \
  -H "Authorization: Bearer your_admin_token"
```

**Notes:**
- Default roles cannot be deleted
- Roles with user assignments cannot be deleted
- Reassign users to different roles before deletion

---

#### POST /api/v1/applications/{appId}/roles/{roleId}/activate

Activate an application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Application role activated successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["Application role is already active."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/applications/app-guid/roles/role-guid/activate \
  -H "Authorization: Bearer your_admin_token"
```

---

#### POST /api/v1/applications/{appId}/roles/{roleId}/deactivate

Deactivate an application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Application role deactivated successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request - Already Inactive:*
```json
{
  "errors": ["Application role is already inactive."]
}
```

*400 Bad Request - Default Role:*
```json
{
  "errors": ["Cannot deactivate the default role."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/applications/app-guid/roles/role-guid/deactivate \
  -H "Authorization: Bearer your_admin_token"
```

**Notes:**
- Default roles cannot be deactivated
- Users with deactivated roles can still login but with limited permissions

---

#### POST /api/v1/applications/{appId}/roles/{roleId}/set-default

Set a role as the default role for the application.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Application role set as default successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request - Inactive:*
```json
{
  "errors": ["Cannot set an inactive role as default."]
}
```

*400 Bad Request - Already Default:*
```json
{
  "errors": ["This role is already the default role."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/applications/app-guid/roles/role-guid/set-default \
  -H "Authorization: Bearer your_admin_token"
```

**Notes:**
- Automatically removes default flag from previous default role
- Default role is assigned to new users when no role is specified
- Each application must have exactly one default role

---

#### POST /api/v1/applications/{appId}/roles/{roleId}/permissions/{permissionId}

Add a permission to an application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID
- `permissionId` (required): Permission GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Permission added to application role successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["Permission is already assigned to this role."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/applications/app-guid/roles/role-guid/permissions/perm-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

#### DELETE /api/v1/applications/{appId}/roles/{roleId}/permissions/{permissionId}

Remove a permission from an application role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appId` (required): Application GUID
- `roleId` (required): Role GUID
- `permissionId` (required): Permission GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Permission removed from application role successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["Application role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["Permission is not assigned to this role."]
}
```

**cURL Example:**
```bash
curl -X DELETE http://localhost:5000/api/v1/applications/app-guid/roles/role-guid/permissions/perm-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

### System Roles Endpoints

System roles are for Auth Admin users only. All endpoints require **Auth Admin** authorization.

#### GET /api/v1/roles

Get paginated list of system roles.

**Authorization:** Auth Admin Only

**Query Parameters:**
- `pageNumber` (optional, default: 1): Page number
- `pageSize` (optional, default: 10, max: 100): Items per page
- `searchTerm` (optional): Search in role name or description
- `isActive` (optional): Filter by active status (true/false)

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "role-guid-1",
      "name": "SuperAdmin",
      "description": "System super administrator with all permissions",
      "isActive": true,
      "userCount": 2,
      "permissionCount": 25,
      "createdAt": "2025-11-09T18:00:00Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalCount": 3,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

**cURL Example:**
```bash
curl -X GET "http://localhost:5000/api/v1/roles?pageSize=20&isActive=true" \
  -H "Authorization: Bearer your_admin_token"
```

---

#### GET /api/v1/roles/{id}

Get detailed information about a specific system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "role-guid",
    "name": "SuperAdmin",
    "description": "System super administrator with all permissions",
    "isActive": true,
    "userCount": 2,
    "createdAt": "2025-11-09T18:00:00Z",
    "updatedAt": "2025-11-09T18:00:00Z",
    "permissions": [
      {
        "id": "perm-guid-1",
        "name": "system.manage",
        "description": "Manage system settings"
      },
      {
        "id": "perm-guid-2",
        "name": "users.manage",
        "description": "Manage all users"
      }
    ],
    "users": [
      {
        "userId": "user-guid-1",
        "email": "admin@genericauth.com",
        "fullName": "System Admin",
        "assignedAt": "2025-11-09T18:00:00Z"
      }
    ]
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["System role not found."]
}
```

**cURL Example:**
```bash
curl -X GET http://localhost:5000/api/v1/roles/role-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

#### POST /api/v1/roles

Create a new system role.

**Authorization:** Auth Admin Only

**Request Body:**
```json
{
  "name": "Support Admin",
  "description": "Support team administrator"
}
```

**Validation Rules:**
- `name`: Required, 1-50 characters, unique
- `description`: Required, 1-200 characters

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "new-role-guid",
    "name": "Support Admin",
    "description": "Support team administrator",
    "isActive": true,
    "createdAt": "2025-11-09T20:00:00Z"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": ["A system role with this name already exists."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/roles \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Support Admin",
    "description": "Support team administrator"
  }'
```

---

#### PUT /api/v1/roles/{id}

Update an existing system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID

**Request Body:**
```json
{
  "name": "Senior Support Admin",
  "description": "Senior support team administrator with elevated permissions"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "role-guid",
    "name": "Senior Support Admin",
    "description": "Senior support team administrator with elevated permissions",
    "isActive": true,
    "updatedAt": "2025-11-09T20:30:00Z"
  }
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["System role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["A system role with this name already exists."]
}
```

**cURL Example:**
```bash
curl -X PUT http://localhost:5000/api/v1/roles/role-guid \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Senior Support Admin",
    "description": "Senior support administrator"
  }'
```

---

#### DELETE /api/v1/roles/{id}

Delete a system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "System role deleted successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["System role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["Cannot delete role with assigned users. Remove user assignments first."]
}
```

**cURL Example:**
```bash
curl -X DELETE http://localhost:5000/api/v1/roles/role-guid \
  -H "Authorization: Bearer your_admin_token"
```

**Notes:**
- Cannot delete roles with user assignments
- Remove all user assignments before deletion

---

#### POST /api/v1/roles/{id}/activate

Activate a system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "System role activated successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["System role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["System role is already active."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/roles/role-guid/activate \
  -H "Authorization: Bearer your_admin_token"
```

---

#### POST /api/v1/roles/{id}/deactivate

Deactivate a system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "System role deactivated successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["System role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["System role is already inactive."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/roles/role-guid/deactivate \
  -H "Authorization: Bearer your_admin_token"
```

---

#### POST /api/v1/roles/{id}/permissions/{permissionId}

Add a permission to a system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID
- `permissionId` (required): Permission GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Permission added to system role successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["System role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["Permission is already assigned to this role."]
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/roles/role-guid/permissions/perm-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

#### DELETE /api/v1/roles/{id}/permissions/{permissionId}

Remove a permission from a system role.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `id` (required): Role GUID
- `permissionId` (required): Permission GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Permission removed from system role successfully."
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["System role not found."]
}
```

*400 Bad Request:*
```json
{
  "errors": ["Permission is not assigned to this role."]
}
```

**cURL Example:**
```bash
curl -X DELETE http://localhost:5000/api/v1/roles/role-guid/permissions/perm-guid \
  -H "Authorization: Bearer your_admin_token"
```

---

### User-Application Assignments Endpoints

All user-application endpoints require **Auth Admin** authorization.

#### POST /api/v1/user-applications

Assign a user to an application with a specific role.

**Authorization:** Auth Admin Only

**Request Body:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "applicationCode": "MY_APP",
  "roleName": "Admin"
}
```

**Fields:**
- `userId` (required): User GUID
- `applicationCode` (required): Application code
- `roleName` (optional): Role name. If not provided, default role is used

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "applicationId": "app-guid",
    "roleId": "role-guid",
    "message": "User assigned to application successfully with role 'Admin'."
  }
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User not found."]
}
```

*400 Bad Request - Already Assigned:*
```json
{
  "errors": ["User is already assigned to this application."]
}
```

*400 Bad Request - Auth Admin:*
```json
{
  "errors": ["Auth Admin users cannot be assigned to applications."]
}
```

**cURL Examples:**

*Assign with specific role:*
```bash
curl -X POST http://localhost:5000/api/v1/user-applications \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-guid",
    "applicationCode": "MY_APP",
    "roleName": "Admin"
  }'
```

*Assign with default role:*
```bash
curl -X POST http://localhost:5000/api/v1/user-applications \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-guid",
    "applicationCode": "MY_APP"
  }'
```

---

#### GET /api/v1/user-applications/users/{userId}/applications

Get all applications assigned to a specific user.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `userId` (required): User GUID

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "applicationId": "app-guid-1",
      "applicationCode": "APP_ONE",
      "applicationName": "Application One",
      "roleId": "role-guid-1",
      "roleName": "Admin",
      "isActive": true,
      "assignedAt": "2025-11-09T19:00:00Z"
    },
    {
      "applicationId": "app-guid-2",
      "applicationCode": "APP_TWO",
      "applicationName": "Application Two",
      "roleId": "role-guid-2",
      "roleName": "User",
      "isActive": true,
      "assignedAt": "2025-11-09T20:00:00Z"
    }
  ]
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["User not found."]
}
```

**cURL Example:**
```bash
curl -X GET http://localhost:5000/api/v1/user-applications/users/user-guid/applications \
  -H "Authorization: Bearer your_admin_token"
```

---

#### GET /api/v1/user-applications/applications/{appCode}/users

Get all users assigned to a specific application (paginated).

**Authorization:** Auth Admin Only

**Path Parameters:**
- `appCode` (required): Application code

**Query Parameters:**
- `pageNumber` (optional, default: 1): Page number
- `pageSize` (optional, default: 10, max: 100): Items per page
- `searchTerm` (optional): Search in email, name, or role
- `isActive` (optional): Filter by active status (true/false)

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "userId": "user-guid-1",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roleId": "role-guid",
      "roleName": "Admin",
      "isActive": true,
      "assignedAt": "2025-11-09T19:00:00Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 42,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["Application not found."]
}
```

**cURL Examples:**

*Get all users:*
```bash
curl -X GET "http://localhost:5000/api/v1/user-applications/applications/MY_APP/users" \
  -H "Authorization: Bearer your_admin_token"
```

*Search users:*
```bash
curl -X GET "http://localhost:5000/api/v1/user-applications/applications/MY_APP/users?searchTerm=john&pageSize=20" \
  -H "Authorization: Bearer your_admin_token"
```

---

#### PUT /api/v1/user-applications/users/{userId}/applications/{appCode}/role

Change a user's role within an application.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `userId` (required): User GUID
- `appCode` (required): Application code

**Request Body:**
```json
{
  "newApplicationRoleId": "new-role-guid"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "user-guid",
    "applicationId": "app-guid",
    "oldRoleId": "old-role-guid",
    "oldRoleName": "User",
    "newRoleId": "new-role-guid",
    "newRoleName": "Admin",
    "message": "User role changed successfully from 'User' to 'Admin'."
  }
}
```

**Error Responses:**

*404 Not Found:*
```json
{
  "errors": ["User application assignment not found."]
}
```

*400 Bad Request - Same Role:*
```json
{
  "errors": ["User already has this role."]
}
```

*400 Bad Request - Inactive Role:*
```json
{
  "errors": ["Cannot assign an inactive role."]
}
```

**cURL Example:**
```bash
curl -X PUT http://localhost:5000/api/v1/user-applications/users/user-guid/applications/MY_APP/role \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "newApplicationRoleId": "new-role-guid"
  }'
```

---

#### DELETE /api/v1/user-applications/users/{userId}/applications/{appCode}

Remove a user from an application.

**Authorization:** Auth Admin Only

**Path Parameters:**
- `userId` (required): User GUID
- `appCode` (required): Application code

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "User removed from application successfully."
}
```

**Error Response (404 Not Found):**
```json
{
  "errors": ["User application assignment not found."]
}
```

**cURL Example:**
```bash
curl -X DELETE http://localhost:5000/api/v1/user-applications/users/user-guid/applications/MY_APP \
  -H "Authorization: Bearer your_admin_token"
```

**Notes:**
- User can no longer login with this application
- All refresh tokens for this user-application combo should be revoked
- User data in application is not deleted (soft delete of assignment)

---

## Data Models

### User

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "userType": "Regular",
  "isActive": true,
  "createdAt": "2025-11-09T18:30:00Z",
  "updatedAt": "2025-11-09T18:30:00Z"
}
```

**Fields:**
- `id` (GUID): Unique user identifier
- `firstName` (string): User's first name
- `lastName` (string): User's last name
- `email` (string): User's email address (unique)
- `userType` (enum): "Regular" or "AuthAdmin"
- `isActive` (boolean): Account status
- `createdAt` (DateTime): Account creation timestamp
- `updatedAt` (DateTime): Last update timestamp

### Application

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "My Application",
  "code": "MY_APP",
  "isActive": true,
  "createdAt": "2025-11-09T18:30:00Z",
  "updatedAt": "2025-11-09T18:30:00Z"
}
```

**Fields:**
- `id` (GUID): Unique application identifier
- `name` (string): Application display name
- `code` (string): Unique application code (uppercase, alphanumeric + underscore)
- `isActive` (boolean): Application status
- `createdAt` (DateTime): Creation timestamp
- `updatedAt` (DateTime): Last update timestamp

### Application Role

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "applicationId": "app-guid",
  "name": "Admin",
  "description": "Application administrator",
  "isDefault": false,
  "isActive": true,
  "createdAt": "2025-11-09T18:30:00Z",
  "updatedAt": "2025-11-09T18:30:00Z"
}
```

**Fields:**
- `id` (GUID): Unique role identifier
- `applicationId` (GUID): Parent application ID
- `name` (string): Role name (unique within application)
- `description` (string): Role description
- `isDefault` (boolean): Is this the default role for new users
- `isActive` (boolean): Role status
- `createdAt` (DateTime): Creation timestamp
- `updatedAt` (DateTime): Last update timestamp

### System Role

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "SuperAdmin",
  "description": "System super administrator",
  "isActive": true,
  "createdAt": "2025-11-09T18:30:00Z",
  "updatedAt": "2025-11-09T18:30:00Z"
}
```

**Fields:**
- `id` (GUID): Unique role identifier
- `name` (string): Role name (globally unique)
- `description` (string): Role description
- `isActive` (boolean): Role status
- `createdAt` (DateTime): Creation timestamp
- `updatedAt` (DateTime): Last update timestamp

### Permission

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "users.read",
  "description": "View users",
  "createdAt": "2025-11-09T18:30:00Z"
}
```

**Fields:**
- `id` (GUID): Unique permission identifier
- `name` (string): Permission name (dot notation recommended)
- `description` (string): Permission description
- `createdAt` (DateTime): Creation timestamp

### User Application Assignment

```json
{
  "userId": "user-guid",
  "applicationId": "app-guid",
  "roleId": "role-guid",
  "assignedAt": "2025-11-09T19:00:00Z"
}
```

**Fields:**
- `userId` (GUID): User identifier
- `applicationId` (GUID): Application identifier
- `roleId` (GUID): Application role identifier
- `assignedAt` (DateTime): Assignment timestamp

### Pagination Response

All paginated endpoints return this structure:

```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 45,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**Pagination Fields:**
- `pageNumber` (int): Current page number (1-based)
- `pageSize` (int): Items per page
- `totalPages` (int): Total number of pages
- `totalCount` (int): Total number of items
- `hasPreviousPage` (boolean): Can navigate to previous page
- `hasNextPage` (boolean): Can navigate to next page

---

## Error Handling

### Standard Error Response Format

All error responses follow this structure:

```json
{
  "errors": [
    "Error message 1",
    "Error message 2"
  ]
}
```

### HTTP Status Codes

| Status Code | Meaning | Usage |
|-------------|---------|-------|
| 200 OK | Success | Successful GET, PUT, POST, DELETE |
| 201 Created | Created | Resource created (currently using 200 for consistency) |
| 400 Bad Request | Client Error | Validation errors, business rule violations |
| 401 Unauthorized | Authentication Required | Missing or invalid JWT token |
| 403 Forbidden | Authorization Failed | Valid token but insufficient permissions |
| 404 Not Found | Resource Not Found | Requested resource doesn't exist |
| 409 Conflict | Resource Conflict | Duplicate unique field (email, code, etc.) |
| 500 Internal Server Error | Server Error | Unexpected server error |

### Common Error Scenarios

#### 401 Unauthorized

**Missing Token:**
```json
{
  "errors": ["Authentication required."]
}
```

**Invalid Token:**
```json
{
  "errors": ["Invalid or expired token."]
}
```

#### 403 Forbidden

**Insufficient Permissions:**
```json
{
  "errors": ["You do not have permission to perform this action."]
}
```

**Wrong User Type:**
```json
{
  "errors": ["This endpoint requires Auth Admin access."]
}
```

#### 400 Bad Request

**Validation Errors:**
```json
{
  "errors": [
    "Email is required.",
    "Password must be at least 8 characters long.",
    "First name must not exceed 50 characters."
  ]
}
```

**Business Rule Violations:**
```json
{
  "errors": [
    "User is already assigned to this application.",
    "Cannot delete role with assigned users."
  ]
}
```

#### 404 Not Found

```json
{
  "errors": ["User not found."]
}
```

#### 500 Internal Server Error

```json
{
  "errors": ["An unexpected error occurred. Please try again later."]
}
```

### Error Handling Best Practices

1. **Always check status code** before parsing response
2. **Display all error messages** to user (they're user-friendly)
3. **Log full error response** for debugging
4. **Implement retry logic** for 500 errors
5. **Refresh tokens** on 401 errors before retrying
6. **Validate input client-side** to reduce 400 errors

### Validation Error Details

FluentValidation errors include:
- Field-level validation (required, length, format)
- Cross-field validation (password confirmation, etc.)
- Business rules (unique email, valid relationships)

Example validation error response:
```json
{
  "errors": [
    "Email is required.",
    "Email must be a valid email address.",
    "Password must be at least 8 characters long.",
    "Password must contain at least one uppercase letter.",
    "Password must contain at least one lowercase letter.",
    "Password must contain at least one digit.",
    "Password must contain at least one special character."
  ]
}
```

---

## Additional Resources

- **Swagger UI**: http://localhost:5000 (when running in development)
- **Health Check**: http://localhost:5000/health
- **Architecture Documentation**: See ARCHITECTURE.md
- **Getting Started Guide**: See GETTING_STARTED.md
- **Postman Collection**: See GenericAuth.postman_collection.json

---

## Version History

### Version 1.0 (Current)
- Initial API release
- Multi-tenant authentication with application-scoped roles
- JWT token authentication with refresh tokens
- Complete CRUD operations for users, applications, roles
- User-application assignment management
- Permission management for both system and application roles
- Comprehensive validation and error handling

---

**Last Updated:** November 9, 2025
**API Version:** v1
**Documentation Version:** 1.0
