# Multi-Tenant Authentication System - Implementation Guide

## 🎯 Overview

This document describes the multi-tenant authentication architecture implemented in GenericAuth. The system supports:

- **Application-scoped authentication**: External applications authenticate with `application_code` + `api_key`
- **Application-scoped roles**: Each application defines its own roles
- **User-Application-Role assignment**: Users have different roles in different applications
- **Auth Admin users**: System-level admins with super privileges
- **Dual role system**: System roles (for Auth Admins) + Application roles (for regular users)

---

## 🏗️ Architecture Decisions

### 1. Dual Role System

**Decision**: Maintain TWO separate role systems instead of a single unified approach.

**Rationale**:
- **System Roles** (existing `Role` entity): For Auth Admin users who manage the entire system
- **Application Roles** (new `ApplicationRole` entity): For regular users within specific applications
- This provides clear separation between system-level and application-level concerns
- Auth Admins can manage all applications without being "assigned" to them

### 2. UserType Enum

Users are classified as:
- **Regular**: Application-scoped users who must authenticate with an application context
- **AuthAdmin**: System-level administrators with super privileges across all applications

### 3. Aggregate Boundaries

- **Application** is the aggregate root for:
  - Application metadata (name, code, API key)
  - ApplicationRoles within that application
  - User assignments to the application

- **User** remains an aggregate root for:
  - User identity and credentials
  - System-level role assignments (for Auth Admins)
  - Application assignments are managed through the Application aggregate

### 4. API Key Security

- API keys are **hashed using SHA-256** before storage (similar to passwords)
- Plain text keys are returned **only once** during generation
- Validation compares hashed values
- Keys are cryptographically generated using `RandomNumberGenerator`

---

## 📦 Domain Layer Implementation

### ✅ Completed Components

#### **Enums**
- `UserType` - Distinguishes Regular users from AuthAdmin users

#### **Value Objects**
1. **ApplicationCode**
   - Validates application codes (3-50 alphanumeric chars, hyphens, underscores)
   - Always stored in uppercase
   - File: `src/GenericAuth.Domain/ValueObjects/ApplicationCode.cs`

2. **ApiKey**
   - Generates cryptographically secure 256-bit keys
   - Hashes keys using SHA-256
   - Validates plain keys against hashed storage
   - File: `src/GenericAuth.Domain/ValueObjects/ApiKey.cs`

#### **Entities**

1. **Application** (Aggregate Root)
   - **Location**: `src/GenericAuth.Domain/Entities/Application.cs`
   - **Properties**:
     - `Name`: Application name
     - `Code`: Unique application code (ApplicationCode VO)
     - `ApiKey`: Hashed API key (ApiKey VO)
     - `IsActive`: Application status
     - `Roles`: Collection of ApplicationRoles
     - `UserApplications`: Users assigned to this application

   - **Key Methods**:
     - `Create()`: Creates app with secure API key, returns (Application, plainApiKey)
     - `UpdateName()`: Updates application name
     - `RegenerateApiKey()`: Rotates API key, returns new plain key
     - `ValidateApiKey()`: Validates plain key against hash
     - `Activate()`/`Deactivate()`: Manage application status
     - `CreateRole()`: Creates role within this application
     - `AssignUser()`: Assigns user to app with specific role
     - `ChangeUserRole()`: Changes user's role within app
     - `RemoveUser()`: Removes user from application

2. **ApplicationRole** (Entity)
   - **Location**: `src/GenericAuth.Domain/Entities/ApplicationRole.cs`
   - **Properties**:
     - `ApplicationId`: Parent application
     - `Name`: Role name (e.g., "Admin", "User", "Viewer")
     - `Description`: Role description
     - `IsActive`: Role status
     - `IsDefault`: Whether this is the default role for new users
     - `Permissions`: Collection of permissions

   - **Key Methods**:
     - `Create()`: Creates application-scoped role
     - `Update()`: Updates name/description
     - `SetAsDefault()`: Marks as default role
     - `Activate()`/`Deactivate()`: Manage role status
     - `AddPermission()`/`RemovePermission()`: Manage permissions

3. **UserApplication** (Join Entity)
   - **Location**: `src/GenericAuth.Domain/Entities/UserApplication.cs`
   - **Properties**:
     - `UserId`: The user
     - `ApplicationId`: The application
     - `ApplicationRoleId`: **User's role within this application**
     - `AssignedAt`: When assigned
     - `AssignedBy`: Who assigned
     - `IsActive`: Assignment status
     - `LastAccessedAt`: Last access timestamp

   - **Key Methods**:
     - `RecordAccess()`: Tracks application access
     - `ChangeRole()`: Changes user's role
     - `Activate()`/`Deactivate()`: Manage assignment status

4. **ApplicationRolePermission** (Join Entity)
   - **Location**: `src/GenericAuth.Domain/Entities/ApplicationRolePermission.cs`
   - Links ApplicationRoles to Permissions (many-to-many)

5. **User** (Updated)
   - **Location**: `src/GenericAuth.Domain/Entities/User.cs`
   - **New Properties**:
     - `UserType`: Regular vs AuthAdmin
     - `UserApplications`: Collection of application assignments

   - **Updated Methods**:
     - `Create()`: Now accepts UserType parameter (defaults to Regular)
     - `CreateAuthAdmin()`: Factory method for Auth Admin users
     - `RecordLogin()`: Now accepts optional applicationId

   - **New Methods**:
     - `IsAuthAdmin()`: Checks if user is an Auth Admin
     - `HasAccessToApplication()`: Checks application access
     - `GetApplicationAssignment()`: Gets user's role in specific app
     - `AddApplicationAssignment()`: Internal method for assignment
     - `RemoveApplicationAssignment()`: Internal method for removal

#### **Domain Events**

All events located in: `src/GenericAuth.Domain/Events/`

1. `ApplicationCreatedEvent` - Raised when application is registered
2. `ApiKeyRotatedEvent` - Raised when API key is regenerated
3. `ApplicationActivatedEvent` - Application activated
4. `ApplicationDeactivatedEvent` - Application deactivated
5. `ApplicationRoleCreatedEvent` - New role created in application
6. `ApplicationRoleSetAsDefaultEvent` - Role set as default
7. `UserAssignedToApplicationEvent` - User assigned to application with role
8. `UserRoleChangedInApplicationEvent` - User's role changed within application
9. `UserRemovedFromApplicationEvent` - User removed from application

---

## 🔄 Authentication Flows

### Flow 1: Application Authentication

**Purpose**: External application authenticates to the auth server

**Process**:
1. Application sends request with headers:
   - `X-Application-Code`: Application code
   - `X-Api-Key`: Plain text API key
2. Middleware validates:
   - Application exists
   - Application is active
   - API key is valid (hash comparison)
3. Application context is added to `HttpContext`
4. Request proceeds

**Status**: ⏳ Middleware implementation pending

---

### Flow 2: User Authentication (Application-Scoped)

**Purpose**: User authenticates FOR a specific application

**Request**:
```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123",
  "application_code": "HR_SYSTEM"
}
```

**Process**:
1. Validate application exists and is active
2. Validate user credentials
3. Check user has access to requested application
4. Get user's role within that application
5. Generate JWT token with application context
6. Return token

**JWT Token Claims**:
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "user_type": "Regular",
  "application_id": "app-guid",
  "application_code": "HR_SYSTEM",
  "application_role_id": "role-guid",
  "application_role_name": "HR_Admin",
  "permissions": ["users.create", "users.read", ...]
}
```

**Auth Admin Exception**:
- Auth Admins can login without `application_code`
- Their tokens don't contain application-specific claims
- They have access to ALL applications

**Status**: ⏳ Implementation pending

---

## 📋 What's Implemented vs Pending

### ✅ COMPLETED: Domain Layer

- [x] UserType enum
- [x] ApplicationCode value object
- [x] ApiKey value object (with secure generation and hashing)
- [x] Application entity (aggregate root)
- [x] ApplicationRole entity
- [x] UserApplication join entity (with ApplicationRoleId)
- [x] ApplicationRolePermission join entity
- [x] Updated User entity with UserType and application methods
- [x] All domain events (9 events)
- [x] Domain layer builds successfully

### ⏳ PENDING: Infrastructure Layer

- [ ] **EF Core Configurations** for:
  - Application entity
  - ApplicationRole entity
  - UserApplication entity (composite key: UserId + ApplicationId)
  - ApplicationRolePermission entity (composite key)
  - Update UserConfiguration to include UserApplications
- [ ] **Database Migration** to add new tables:
  - `Applications`
  - `ApplicationRoles`
  - `UserApplications`
  - `ApplicationRolePermissions`
  - Add `UserType` column to `Users` table
- [ ] **Update Repository Interfaces** to include application queries

### ⏳ PENDING: Application Layer (CQRS)

#### Commands & Handlers

1. **Application Management**:
   - `CreateApplicationCommand` - Register new application with initial roles
   - `UpdateApplicationCommand` - Update application details
   - `RegenerateApiKeyCommand` - Rotate API key
   - `ActivateApplicationCommand` / `DeactivateApplicationCommand`

2. **Application Role Management**:
   - `CreateApplicationRoleCommand` - Add role to application
   - `UpdateApplicationRoleCommand`
   - `SetDefaultApplicationRoleCommand`
   - `ActivateApplicationRoleCommand` / `DeactivateApplicationRoleCommand`

3. **User-Application Assignment**:
   - `AssignUserToApplicationCommand` - Assign user to app with role
   - `ChangeUserApplicationRoleCommand` - Change user's role in app
   - `RemoveUserFromApplicationCommand` - Remove user from app

4. **Authentication**:
   - `AuthenticateUserCommand` - Updated to include application context
   - `AuthenticateApplicationCommand` - Validate app code + API key

#### Queries & Handlers

1. **Application Queries**:
   - `GetApplicationsQuery` - List all applications
   - `GetApplicationByIdQuery`
   - `GetApplicationByCodeQuery` - Critical for authentication
   - `GetApplicationRolesQuery` - Get roles for an application

2. **User-Application Queries**:
   - `GetUserApplicationsQuery` - Get all applications a user has access to
   - `GetApplicationUsersQuery` - Get all users in an application
   - `GetUserRoleInApplicationQuery` - Get user's role in specific app

#### Validators

- FluentValidation validators for all commands

### ⏳ PENDING: API Layer

#### Controllers

1. **ApplicationsController** (Auth Admin only):
   ```
   POST   /api/applications                          - Register application
   GET    /api/applications                          - List applications
   GET    /api/applications/{id}                     - Get application
   PUT    /api/applications/{id}                     - Update application
   POST   /api/applications/{id}/regenerate-api-key  - Rotate API key
   POST   /api/applications/{id}/activate            - Activate
   POST   /api/applications/{id}/deactivate          - Deactivate
   ```

2. **ApplicationRolesController** (Auth Admin only):
   ```
   POST   /api/applications/{appId}/roles            - Create role
   GET    /api/applications/{appId}/roles            - List roles
   PUT    /api/applications/{appId}/roles/{roleId}   - Update role
   POST   /api/applications/{appId}/roles/{roleId}/set-default
   ```

3. **UserApplicationsController** (Auth Admin only):
   ```
   POST   /api/user-applications                     - Assign user to app
   GET    /api/users/{userId}/applications           - User's applications
   GET    /api/applications/{appId}/users            - Application's users
   PUT    /api/user-applications/{userId}/applications/{appId}/role
   DELETE /api/user-applications/{userId}/applications/{appId}
   ```

4. **AuthController** (Updated):
   ```
   POST   /api/auth/login        - Login with application context
   POST   /api/auth/refresh      - Refresh token
   POST   /api/auth/logout       - Logout
   ```

#### Middleware

1. **ApplicationAuthenticationMiddleware**:
   - Validates `X-Application-Code` and `X-Api-Key` headers
   - Loads application context into HttpContext
   - Rejects invalid/inactive applications

2. **Updated JWT Middleware**:
   - Validates application-scoped tokens
   - Populates HttpContext with application claims

#### Authorization Policies

```csharp
// Examples
[Authorize(Policy = "AuthAdminOnly")]
[Authorize(Policy = "RequireApplication")]
[Authorize(Policy = "RequireApplicationRole:Admin")]
[Authorize(Policy = "RequirePermission:users.create")]
```

### ⏳ PENDING: Data Migration Strategy

#### Approach

1. **Create "System" Application**:
   - Application Code: `SYSTEM`
   - Name: "System Administration"
   - For Auth Admin operations

2. **Create Default Application** (optional):
   - Application Code: `DEFAULT`
   - Name: "Default Application"
   - Migrate existing users here if needed

3. **Migrate Existing Users**:
   - **Option A**: Set all as UserType.Regular, assign to DEFAULT app
   - **Option B**: Identify admins, set as UserType.AuthAdmin
   - **Recommended**: Option B - identify admin users and upgrade them

4. **Migration Script**:
   - Add UserType column (default: Regular)
   - Create SYSTEM application
   - Identify and update admin users to AuthAdmin type
   - Optionally create DEFAULT app and assign users

---

## 🔐 Security Considerations

### API Key Management

✅ **Implemented**:
- Keys are hashed using SHA-256 before storage
- Cryptographically secure generation (256 bits)
- Plain text keys shown only once during creation
- Validation uses constant-time comparison via hash check

⚠️ **TODO**:
- Consider key rotation policies
- Add key expiration/renewal
- Track key usage for auditing
- Rate limiting for API key validation

### Authentication

✅ **Design**:
- Application authentication separate from user authentication
- Multi-level validation (app → user → role)
- JWT tokens contain application context

⚠️ **TODO**:
- Implement refresh token rotation
- Add token revocation list
- Rate limiting on login endpoints
- Account lockout after failed attempts

### Authorization

✅ **Design**:
- Application-scoped roles prevent cross-application access
- Auth Admins segregated from regular users
- Permission-based access control

⚠️ **TODO**:
- Implement authorization policies
- Add permission caching
- Audit logging for authorization failures

---

## 📊 Database Schema (Conceptual)

### New Tables

```sql
-- Applications table
Applications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200),
    Code NVARCHAR(50) UNIQUE,  -- Stored as uppercase
    ApiKeyHash NVARCHAR(MAX),   -- SHA-256 hash
    ApiKeyGeneratedAt DATETIME2,
    IsActive BIT,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
)

-- Application-specific roles
ApplicationRoles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationId UNIQUEIDENTIFIER FOREIGN KEY,
    Name NVARCHAR(100),
    Description NVARCHAR(500),
    IsActive BIT,
    IsDefault BIT,
    CreatedAt DATETIME2,
    ...
)

-- User-Application assignments with role
UserApplications (
    UserId UNIQUEIDENTIFIER,
    ApplicationId UNIQUEIDENTIFIER,
    ApplicationRoleId UNIQUEIDENTIFIER FOREIGN KEY,
    AssignedAt DATETIME2,
    AssignedBy NVARCHAR(255),
    IsActive BIT,
    LastAccessedAt DATETIME2,
    UpdatedAt DATETIME2,
    PRIMARY KEY (UserId, ApplicationId)
)

-- Application Role Permissions
ApplicationRolePermissions (
    ApplicationRoleId UNIQUEIDENTIFIER,
    PermissionId UNIQUEIDENTIFIER,
    AssignedAt DATETIME2,
    PRIMARY KEY (ApplicationRoleId, PermissionId)
)
```

### Updated Tables

```sql
-- Add UserType to Users table
ALTER TABLE Users ADD UserType INT DEFAULT 0;  -- 0=Regular, 1=AuthAdmin
```

---

## 🚀 Next Steps (Implementation Order)

### Phase 1: Infrastructure Layer
1. Create EF Core configurations for new entities
2. Generate and apply database migration
3. Update DbContext to include new DbSets
4. Test entity mappings

### Phase 2: Application Layer - Queries
1. Implement GetApplicationByCodeQuery (critical for auth)
2. Implement GetApplicationsQuery
3. Implement GetUserApplicationsQuery
4. Implement GetApplicationRolesQuery

### Phase 3: Application Layer - Commands
1. Implement CreateApplicationCommand
2. Implement CreateApplicationRoleCommand
3. Implement AssignUserToApplicationCommand
4. Implement AuthenticateUserCommand (updated with app context)

### Phase 4: Middleware & Authentication
1. Create ApplicationAuthenticationMiddleware
2. Update JWT token service for application claims
3. Update authentication flow
4. Create authorization policies

### Phase 5: API Controllers
1. Create ApplicationsController
2. Create ApplicationRolesController
3. Create UserApplicationsController
4. Update AuthController

### Phase 6: Data Migration & Seeding
1. Create migration script
2. Seed Auth Admin user
3. Seed SYSTEM application
4. Test migration with sample data

### Phase 7: Testing
1. Unit tests for domain logic
2. Integration tests for repositories
3. API integration tests
4. End-to-end authentication flow tests

---

## 📝 Usage Examples

### Example 1: Registering an Application (Auth Admin)

```csharp
// Command
var command = new CreateApplicationCommand
{
    Name = "HR Management System",
    Code = "HR_SYSTEM",
    InitialRoles = new[]
    {
        new { Name = "HR_Admin", Description = "HR Administrator", IsDefault = false },
        new { Name = "HR_User", Description = "HR User", IsDefault = false },
        new { Name = "Employee", Description = "Employee Self-Service", IsDefault = true }
    }
};

// Handler creates application and returns plain API key
var result = await mediator.Send(command);
// result.ApiKey = "abc123..." (show this ONCE to the user)
```

### Example 2: Assigning User to Application

```csharp
var command = new AssignUserToApplicationCommand
{
    UserId = userGuid,
    ApplicationCode = "HR_SYSTEM",
    RoleName = "HR_User"
};

await mediator.Send(command);
```

### Example 3: User Login (Application-Scoped)

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@company.com",
  "password": "SecurePass123!",
  "application_code": "HR_SYSTEM"
}
```

**Response**:
```json
{
  "token": "eyJhbGc...",
  "refreshToken": "...",
  "expiresIn": 3600,
  "user": {
    "id": "...",
    "email": "john@company.com",
    "firstName": "John",
    "lastName": "Doe",
    "application": {
      "code": "HR_SYSTEM",
      "name": "HR Management System",
      "role": "HR_User"
    }
  }
}
```

### Example 4: External Application Authenticating

```http
GET /api/some-protected-endpoint
X-Application-Code: HR_SYSTEM
X-Api-Key: abc123def456...
Authorization: Bearer {user-jwt-token}
```

---

## ✅ Summary

### What We Built

A comprehensive **multi-tenant authentication system** with:

- **Dual role architecture**: System roles + Application roles
- **Secure API key management**: Hashed storage, one-time display
- **Application-scoped authentication**: Users authenticate FOR applications
- **Rich domain model**: Full DDD implementation with aggregates, value objects, and events
- **Flexible user assignment**: Users can have different roles in different applications
- **Auth Admin segregation**: System administrators with super privileges

### What's Left

- **Infrastructure**: EF Core configs + migrations
- **Application**: CQRS implementation (commands + queries)
- **API**: Controllers + middleware + authorization policies
- **Migration**: Data migration strategy for existing users

### Domain Layer Status

✅ **100% Complete and Verified**
- All entities, value objects, and events implemented
- Builds successfully with zero errors
- Ready for infrastructure layer implementation

---

## 📞 Questions or Issues?

Refer to individual entity files for detailed implementation and XML documentation.

**Next Action**: Proceed with Phase 1 (Infrastructure Layer) - EF Core configurations and migrations.
