# Multi-Tenant Authentication System - Comprehensive Architecture Plan

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Domain Model Design](#domain-model-design)
3. [Authentication Flow](#authentication-flow)
4. [Security Considerations](#security-considerations)
5. [Data Model & Database Schema](#data-model--database-schema)
6. [API Design](#api-design)
7. [Implementation Strategy by Layer](#implementation-strategy-by-layer)
8. [Migration Strategy](#migration-strategy)
9. [Code Structure & Organization](#code-structure--organization)
10. [Potential Pitfalls & Considerations](#potential-pitfalls--considerations)

---

## Executive Summary

This document outlines a comprehensive architectural plan to transform the existing authentication system into a **multi-tenant authentication service** that can serve multiple external applications. The design follows DDD principles, Clean Architecture, and maintains the existing CQRS pattern with MediatR.

### Key Architectural Decisions:
- **Application as Aggregate Root**: Application is a separate aggregate managing its own consistency boundary
- **User-Application Relationship**: Many-to-many via UserApplication join entity with application-specific user metadata
- **Roles & Permissions**: Per-application isolation (each application has its own role/permission structure)
- **APIKey Security**: Hashed storage using SHA256, never stored in plain text
- **SecretCode**: Encrypted storage for sensitive operations (key rotation, webhooks)
- **Multi-Application Auth**: Support for validating multiple ApplicationCode+APIKey pairs in a single request

---

## 1. Domain Model Design

### 1.1 Application Aggregate Root

The `Application` entity serves as an aggregate root representing a tenant in the system.

**Entity Definition:**

```csharp
// GenericAuth.Domain/Entities/Application.cs
public class Application : BaseEntity
{
    // Basic Information
    public string ApplicationName { get; private set; }
    public ApplicationCode ApplicationCode { get; private set; } // Value Object
    public string SecretCodeHash { get; private set; } // Encrypted
    public string ApiKeyHash { get; private set; } // Hashed

    // Status & Configuration
    public bool IsActive { get; private set; }
    public bool RequireEmailConfirmation { get; private set; }
    public ApplicationSettings Settings { get; private set; } // Value Object

    // Rate Limiting & Security
    public int MaxRequestsPerMinute { get; private set; }
    public int MaxFailedAttemptsBeforeLock { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Audit
    public DateTime? LastAccessedAt { get; private set; }
    public int TotalRequestCount { get; private set; }

    // Relationships
    private readonly List<UserApplication> _userApplications = new();
    public IReadOnlyCollection<UserApplication> UserApplications => _userApplications.AsReadOnly();

    private readonly List<ApplicationRole> _roles = new();
    public IReadOnlyCollection<ApplicationRole> Roles => _roles.AsReadOnly();

    private readonly List<ApplicationPermission> _permissions = new();
    public IReadOnlyCollection<ApplicationPermission> Permissions => _permissions.AsReadOnly();

    private readonly List<ApplicationAccessLog> _accessLogs = new();
    public IReadOnlyCollection<ApplicationAccessLog> AccessLogs => _accessLogs.AsReadOnly();

    // Factory Method
    public static Application Create(
        string applicationName,
        string applicationCode,
        string secretCode,
        string apiKey,
        IApplicationSecurityService securityService)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new DomainException("Application name cannot be empty.");

        var appCode = ApplicationCode.Create(applicationCode);
        var apiKeyHash = securityService.HashApiKey(apiKey);
        var secretCodeHash = securityService.EncryptSecretCode(secretCode);

        var application = new Application(
            applicationName,
            appCode,
            secretCodeHash,
            apiKeyHash);

        application.AddDomainEvent(new ApplicationRegisteredEvent(application.Id, application.ApplicationCode));

        return application;
    }

    // Domain Behavior
    public void RegisterUser(User user, IEnumerable<Guid> roleIds)
    {
        if (!IsActive)
            throw new DomainException("Cannot register users to inactive application.");

        var userApp = UserApplication.Create(user.Id, Id, roleIds);
        _userApplications.Add(userApp);

        AddDomainEvent(new UserRegisteredToApplicationEvent(Id, user.Id));
        SetUpdatedInfo();
    }

    public void ValidateApiKey(string apiKey, IApplicationSecurityService securityService)
    {
        if (!IsActive)
            throw new DomainException($"Application '{ApplicationName}' is inactive.");

        if (IsLocked())
            throw new DomainException($"Application is locked until {LockedUntil}.");

        if (!securityService.VerifyApiKey(apiKey, ApiKeyHash))
        {
            RecordFailedAttempt();
            throw new DomainException("Invalid API key.");
        }

        RecordSuccessfulAccess();
    }

    public void RotateApiKey(string newApiKey, IApplicationSecurityService securityService)
    {
        ApiKeyHash = securityService.HashApiKey(newApiKey);
        AddDomainEvent(new ApiKeyRotatedEvent(Id, ApplicationCode));
        SetUpdatedInfo();
    }

    public void RotateSecretCode(string newSecretCode, IApplicationSecurityService securityService)
    {
        SecretCodeHash = securityService.EncryptSecretCode(newSecretCode);
        AddDomainEvent(new SecretCodeRotatedEvent(Id, ApplicationCode));
        SetUpdatedInfo();
    }

    public void UpdateSettings(ApplicationSettings settings)
    {
        Settings = settings;
        SetUpdatedInfo();
    }

    public bool IsLocked() => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;

    private void RecordFailedAttempt()
    {
        // Implement exponential backoff or lock mechanism
        // This is a simplified version
    }

    private void RecordSuccessfulAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
        TotalRequestCount++;
    }
}
```

### 1.2 Value Objects

**ApplicationCode Value Object:**

```csharp
// GenericAuth.Domain/ValueObjects/ApplicationCode.cs
public sealed class ApplicationCode : ValueObject
{
    private const int MinLength = 3;
    private const int MaxLength = 50;
    private static readonly Regex CodePattern = new Regex(@"^[a-zA-Z0-9_-]+$");

    public string Value { get; private set; }

    private ApplicationCode(string value)
    {
        Value = value;
    }

    public static ApplicationCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Application code cannot be empty.");

        if (code.Length < MinLength || code.Length > MaxLength)
            throw new DomainException($"Application code must be between {MinLength} and {MaxLength} characters.");

        if (!CodePattern.IsMatch(code))
            throw new DomainException("Application code can only contain alphanumeric characters, hyphens, and underscores.");

        return new ApplicationCode(code.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ApplicationCode code) => code.Value;
}
```

**ApplicationSettings Value Object:**

```csharp
// GenericAuth.Domain/ValueObjects/ApplicationSettings.cs
public sealed class ApplicationSettings : ValueObject
{
    public int TokenExpirationMinutes { get; private set; }
    public int RefreshTokenExpirationDays { get; private set; }
    public bool AllowMultipleSessions { get; private set; }
    public bool EnableTwoFactorAuth { get; private set; }
    public string? CallbackUrl { get; private set; }
    public string? WebhookUrl { get; private set; }

    private ApplicationSettings(
        int tokenExpirationMinutes,
        int refreshTokenExpirationDays,
        bool allowMultipleSessions,
        bool enableTwoFactorAuth,
        string? callbackUrl,
        string? webhookUrl)
    {
        TokenExpirationMinutes = tokenExpirationMinutes;
        RefreshTokenExpirationDays = refreshTokenExpirationDays;
        AllowMultipleSessions = allowMultipleSessions;
        EnableTwoFactorAuth = enableTwoFactorAuth;
        CallbackUrl = callbackUrl;
        WebhookUrl = webhookUrl;
    }

    public static ApplicationSettings CreateDefault()
    {
        return new ApplicationSettings(
            tokenExpirationMinutes: 60,
            refreshTokenExpirationDays: 7,
            allowMultipleSessions: true,
            enableTwoFactorAuth: false,
            callbackUrl: null,
            webhookUrl: null);
    }

    public static ApplicationSettings Create(
        int tokenExpirationMinutes,
        int refreshTokenExpirationDays,
        bool allowMultipleSessions,
        bool enableTwoFactorAuth,
        string? callbackUrl = null,
        string? webhookUrl = null)
    {
        if (tokenExpirationMinutes < 5 || tokenExpirationMinutes > 1440)
            throw new DomainException("Token expiration must be between 5 and 1440 minutes.");

        if (refreshTokenExpirationDays < 1 || refreshTokenExpirationDays > 90)
            throw new DomainException("Refresh token expiration must be between 1 and 90 days.");

        return new ApplicationSettings(
            tokenExpirationMinutes,
            refreshTokenExpirationDays,
            allowMultipleSessions,
            enableTwoFactorAuth,
            callbackUrl,
            webhookUrl);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TokenExpirationMinutes;
        yield return RefreshTokenExpirationDays;
        yield return AllowMultipleSessions;
        yield return EnableTwoFactorAuth;
        yield return CallbackUrl;
        yield return WebhookUrl;
    }
}
```

### 1.3 User-Application Relationship

**UserApplication Join Entity with Rich Behavior:**

```csharp
// GenericAuth.Domain/Entities/UserApplication.cs
public class UserApplication : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } // Navigation

    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; } // Navigation

    // Application-specific user data
    public string? ApplicationSpecificUserId { get; private set; } // External ID in the application
    public bool IsActive { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }
    public int AccessCount { get; private set; }

    // Application-specific roles (many-to-many)
    private readonly List<UserApplicationRole> _userApplicationRoles = new();
    public IReadOnlyCollection<UserApplicationRole> UserApplicationRoles => _userApplicationRoles.AsReadOnly();

    private UserApplication() { }

    private UserApplication(Guid userId, Guid applicationId, IEnumerable<Guid> roleIds)
    {
        UserId = userId;
        ApplicationId = applicationId;
        IsActive = true;

        foreach (var roleId in roleIds)
        {
            _userApplicationRoles.Add(new UserApplicationRole
            {
                UserApplicationId = Id,
                ApplicationRoleId = roleId
            });
        }
    }

    public static UserApplication Create(Guid userId, Guid applicationId, IEnumerable<Guid> roleIds)
    {
        return new UserApplication(userId, applicationId, roleIds);
    }

    public void SetApplicationSpecificUserId(string externalId)
    {
        ApplicationSpecificUserId = externalId;
        SetUpdatedInfo();
    }

    public void RecordAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
        AccessCount++;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("User-Application relationship is already active.");
        IsActive = true;
        SetUpdatedInfo();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("User-Application relationship is already inactive.");
        IsActive = false;
        SetUpdatedInfo();
    }

    public void AddRole(Guid applicationRoleId)
    {
        if (_userApplicationRoles.Any(r => r.ApplicationRoleId == applicationRoleId))
            throw new DomainException("User already has this role in the application.");

        _userApplicationRoles.Add(new UserApplicationRole
        {
            UserApplicationId = Id,
            ApplicationRoleId = applicationRoleId
        });
        SetUpdatedInfo();
    }

    public void RemoveRole(Guid applicationRoleId)
    {
        var role = _userApplicationRoles.FirstOrDefault(r => r.ApplicationRoleId == applicationRoleId);
        if (role == null)
            throw new DomainException("User does not have this role in the application.");

        _userApplicationRoles.Remove(role);
        SetUpdatedInfo();
    }
}
```

### 1.4 Application-Scoped Roles & Permissions

**Important Decision:** Roles and Permissions are **per-application** (not global). This provides complete tenant isolation.

```csharp
// GenericAuth.Domain/Entities/ApplicationRole.cs
public class ApplicationRole : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ApplicationRolePermission> _rolePermissions = new();
    public IReadOnlyCollection<ApplicationRolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private readonly List<UserApplicationRole> _userApplicationRoles = new();
    public IReadOnlyCollection<UserApplicationRole> UserApplicationRoles => _userApplicationRoles.AsReadOnly();

    public static ApplicationRole Create(Guid applicationId, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        return new ApplicationRole
        {
            ApplicationId = applicationId,
            Name = name,
            Description = description,
            IsActive = true
        };
    }

    // Similar methods as Role entity...
}

// GenericAuth.Domain/Entities/ApplicationPermission.cs
public class ApplicationPermission : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ApplicationRolePermission> _rolePermissions = new();
    public IReadOnlyCollection<ApplicationRolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public static ApplicationPermission Create(
        Guid applicationId,
        string name,
        string description,
        string resource,
        string action)
    {
        // Validation logic similar to Permission entity
        return new ApplicationPermission
        {
            ApplicationId = applicationId,
            Name = name,
            Description = description,
            Resource = resource,
            Action = action,
            IsActive = true
        };
    }
}

// Join tables
public class ApplicationRolePermission
{
    public Guid ApplicationRoleId { get; set; }
    public ApplicationRole ApplicationRole { get; set; }

    public Guid ApplicationPermissionId { get; set; }
    public ApplicationPermission ApplicationPermission { get; set; }
}

public class UserApplicationRole
{
    public Guid UserApplicationId { get; set; }
    public UserApplication UserApplication { get; set; }

    public Guid ApplicationRoleId { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
}
```

### 1.5 Domain Events

```csharp
// GenericAuth.Domain/Events/ApplicationRegisteredEvent.cs
public record ApplicationRegisteredEvent(Guid ApplicationId, ApplicationCode ApplicationCode) : BaseDomainEvent;

// GenericAuth.Domain/Events/UserRegisteredToApplicationEvent.cs
public record UserRegisteredToApplicationEvent(Guid ApplicationId, Guid UserId) : BaseDomainEvent;

// GenericAuth.Domain/Events/ApplicationValidatedEvent.cs
public record ApplicationValidatedEvent(Guid ApplicationId, string ApplicationCode) : BaseDomainEvent;

// GenericAuth.Domain/Events/ApiKeyRotatedEvent.cs
public record ApiKeyRotatedEvent(Guid ApplicationId, ApplicationCode ApplicationCode) : BaseDomainEvent;

// GenericAuth.Domain/Events/SecretCodeRotatedEvent.cs
public record SecretCodeRotatedEvent(Guid ApplicationId, ApplicationCode ApplicationCode) : BaseDomainEvent;

// GenericAuth.Domain/Events/ApplicationAccessGrantedEvent.cs
public record ApplicationAccessGrantedEvent(
    Guid ApplicationId,
    Guid UserId,
    string IpAddress,
    DateTime AccessedAt) : BaseDomainEvent;

// GenericAuth.Domain/Events/ApplicationAccessDeniedEvent.cs
public record ApplicationAccessDeniedEvent(
    string ApplicationCode,
    string Reason,
    string IpAddress,
    DateTime AttemptedAt) : BaseDomainEvent;
```

### 1.6 Domain Services

```csharp
// GenericAuth.Domain/Services/IApplicationSecurityService.cs
public interface IApplicationSecurityService
{
    string HashApiKey(string apiKey);
    bool VerifyApiKey(string apiKey, string hash);
    string EncryptSecretCode(string secretCode);
    string DecryptSecretCode(string encryptedSecretCode);
    string GenerateApiKey();
    string GenerateSecretCode();
}
```

---

## 2. Authentication Flow

### 2.1 Multi-Tenant Authentication Flow

```
┌─────────────────┐
│  External App   │
└────────┬────────┘
         │
         │ 1. Login Request
         │ Headers:
         │   X-Application-Code: APP001
         │   X-API-Key: abc123...
         │ Body:
         │   { email, password }
         │
         ▼
┌─────────────────────────────────────────────────┐
│         API Gateway / Middleware                │
│  ┌───────────────────────────────────────────┐  │
│  │  ApplicationAuthenticationMiddleware      │  │
│  │  - Extract ApplicationCode + APIKey       │  │
│  │  - Validate Application                   │  │
│  │  - Set ApplicationContext                 │  │
│  └───────────────────────────────────────────┘  │
└────────┬────────────────────────────────────────┘
         │
         │ 2. Validated Request + ApplicationContext
         │
         ▼
┌─────────────────────────────────────────────────┐
│         LoginCommandHandler                     │
│  ┌───────────────────────────────────────────┐  │
│  │  1. Validate User Credentials             │  │
│  │  2. Check User belongs to Application     │  │
│  │  3. Check UserApplication.IsActive        │  │
│  │  4. Load Application-specific Roles       │  │
│  │  5. Generate JWT with App Context         │  │
│  │  6. Record Access in UserApplication      │  │
│  └───────────────────────────────────────────┘  │
└────────┬────────────────────────────────────────┘
         │
         │ 3. JWT Token + Refresh Token
         │ Token Claims:
         │   - sub: UserId
         │   - email: user@example.com
         │   - app_code: APP001
         │   - app_id: <ApplicationId>
         │   - roles: [Admin, Editor] (app-specific)
         │   - permissions: [...]
         │
         ▼
┌─────────────────┐
│  External App   │
│  Stores Token   │
└─────────────────┘
```

### 2.2 Multiple Application Authentication

For scenarios where a single request needs to validate against multiple applications:

```csharp
// Request with multiple app credentials
POST /auth/multi-app-login
Headers:
  X-Application-Credentials: [
    { "applicationCode": "APP001", "apiKey": "key1..." },
    { "applicationCode": "APP002", "apiKey": "key2..." }
  ]
Body:
  { "email": "user@example.com", "password": "..." }

// Response with multiple tokens
{
  "applications": [
    {
      "applicationCode": "APP001",
      "token": "jwt-for-app001...",
      "refreshToken": "...",
      "roles": ["Admin"],
      "expiresAt": "..."
    },
    {
      "applicationCode": "APP002",
      "token": "jwt-for-app002...",
      "refreshToken": "...",
      "roles": ["User"],
      "expiresAt": "..."
    }
  ]
}
```

### 2.3 Detailed Authentication Steps

**Step 1: Application Validation (Middleware)**

```csharp
public class ApplicationAuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context, IApplicationAuthService authService)
    {
        // Extract credentials
        var appCode = context.Request.Headers["X-Application-Code"].FirstOrDefault();
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(appCode) || string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Missing application credentials" });
            return;
        }

        // Validate application
        var validationResult = await authService.ValidateApplicationAsync(appCode, apiKey);

        if (!validationResult.IsSuccess)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = validationResult.Error });
            return;
        }

        // Set application context for downstream handlers
        context.Items["ApplicationId"] = validationResult.Value.ApplicationId;
        context.Items["ApplicationCode"] = validationResult.Value.ApplicationCode;
        context.Items["Application"] = validationResult.Value;

        await _next(context);
    }
}
```

**Step 2: User Authentication with Application Context**

```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    public async Task<Result<LoginCommandResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Get application context from middleware
        var applicationId = _httpContextAccessor.HttpContext.Items["ApplicationId"] as Guid?;
        if (!applicationId.HasValue)
            return Result<LoginCommandResponse>.Failure("Application context not found.");

        // 1. Find user by email
        var user = await _context.Users
            .Include(u => u.UserApplications.Where(ua => ua.ApplicationId == applicationId))
            .ThenInclude(ua => ua.UserApplicationRoles)
            .ThenInclude(uar => uar.ApplicationRole)
            .FirstOrDefaultAsync(u => u.Email == Email.Create(request.Email), cancellationToken);

        if (user == null)
            return Result<LoginCommandResponse>.Failure("Invalid email or password.");

        // 2. Verify password
        if (!_passwordHasher.Verify(request.Password, user.Password.Hash))
            return Result<LoginCommandResponse>.Failure("Invalid email or password.");

        // 3. Check user is active
        if (!user.IsActive)
            return Result<LoginCommandResponse>.Failure("User account is inactive.");

        // 4. Check user belongs to this application
        var userApplication = user.UserApplications.FirstOrDefault(ua => ua.ApplicationId == applicationId);
        if (userApplication == null)
            return Result<LoginCommandResponse>.Failure("User not registered for this application.");

        if (!userApplication.IsActive)
            return Result<LoginCommandResponse>.Failure("User access revoked for this application.");

        // 5. Get application
        var application = await _context.Applications
            .Include(a => a.Settings)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        // 6. Get application-specific roles and permissions
        var roles = userApplication.UserApplicationRoles
            .Select(uar => uar.ApplicationRole.Name)
            .ToList();

        var permissions = userApplication.UserApplicationRoles
            .SelectMany(uar => uar.ApplicationRole.RolePermissions)
            .Select(rp => rp.ApplicationPermission)
            .Select(p => new PermissionDto(p.Resource, p.Action))
            .Distinct()
            .ToList();

        // 7. Generate JWT with application context
        var token = _jwtTokenGenerator.GenerateToken(
            user,
            application,
            roles,
            permissions);

        // 8. Generate refresh token
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(
            refreshTokenString,
            validityInDays: application.Settings.RefreshTokenExpirationDays);
        user.AddRefreshToken(refreshToken);

        // 9. Record access
        user.RecordLogin();
        userApplication.RecordAccess();

        // 10. Raise domain event
        user.AddDomainEvent(new ApplicationAccessGrantedEvent(
            application.Id,
            user.Id,
            _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            DateTime.UtcNow));

        // 11. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 12. Return response
        var response = new LoginCommandResponse(
            Token: token,
            RefreshToken: refreshTokenString,
            ExpiresAt: DateTime.UtcNow.AddMinutes(application.Settings.TokenExpirationMinutes),
            User: new UserDto(user.Id, user.FirstName, user.LastName, user.Email.Value),
            Application: new ApplicationDto(application.Id, application.ApplicationName, application.ApplicationCode),
            Roles: roles,
            Permissions: permissions);

        return Result<LoginCommandResponse>.Success(response);
    }
}
```

**Step 3: JWT Token Structure**

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-guid",
    "email": "user@example.com",
    "given_name": "John",
    "family_name": "Doe",
    "jti": "unique-token-id",
    "app_id": "application-guid",
    "app_code": "APP001",
    "app_name": "My External App",
    "roles": ["Admin", "Editor"],
    "permissions": [
      { "resource": "users", "action": "read" },
      { "resource": "users", "action": "write" }
    ],
    "iss": "GenericAuth",
    "aud": "APP001",
    "exp": 1699999999,
    "iat": 1699996399
  }
}
```

---

## 3. Security Considerations

### 3.1 APIKey Storage & Validation

**Hashing Strategy:**

```csharp
// GenericAuth.Infrastructure/Security/ApplicationSecurityService.cs
public class ApplicationSecurityService : IApplicationSecurityService
{
    private readonly IConfiguration _configuration;

    public string HashApiKey(string apiKey)
    {
        // Use SHA256 with salt for API key hashing
        using var sha256 = SHA256.Create();
        var salt = _configuration["Security:ApiKeySalt"]
            ?? throw new InvalidOperationException("API Key salt not configured");
        var combined = apiKey + salt;
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyApiKey(string apiKey, string hash)
    {
        var computedHash = HashApiKey(apiKey);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(hash));
    }

    public string GenerateApiKey()
    {
        // Generate cryptographically secure random API key
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```

**Important:** Never store plain-text API keys. Always hash before storage.

### 3.2 SecretCode Storage & Usage

**Encryption Strategy:**

```csharp
public string EncryptSecretCode(string secretCode)
{
    // Use AES encryption for secret code (needs to be reversible for rotation)
    var key = _configuration["Security:EncryptionKey"]
        ?? throw new InvalidOperationException("Encryption key not configured");

    using var aes = Aes.Create();
    aes.Key = Convert.FromBase64String(key);
    aes.GenerateIV();

    using var encryptor = aes.CreateEncryptor();
    var plainBytes = Encoding.UTF8.GetBytes(secretCode);
    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

    // Prepend IV to encrypted data
    var result = new byte[aes.IV.Length + encryptedBytes.Length];
    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
    Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

    return Convert.ToBase64String(result);
}

public string DecryptSecretCode(string encryptedSecretCode)
{
    var key = _configuration["Security:EncryptionKey"]
        ?? throw new InvalidOperationException("Encryption key not configured");

    var fullCipher = Convert.FromBase64String(encryptedSecretCode);

    using var aes = Aes.Create();
    aes.Key = Convert.FromBase64String(key);

    // Extract IV from prepended bytes
    var iv = new byte[aes.IV.Length];
    var cipher = new byte[fullCipher.Length - iv.Length];

    Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
    Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

    aes.IV = iv;

    using var decryptor = aes.CreateDecryptor();
    var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

    return Encoding.UTF8.GetString(decryptedBytes);
}

public string GenerateSecretCode()
{
    // Generate strong secret code (64 characters)
    var bytes = new byte[48];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes);
}
```

**Usage Scenarios for SecretCode:**
1. Webhook signature verification
2. API key rotation requests
3. Application-to-application secure communication
4. Sensitive configuration changes

### 3.3 Rate Limiting

**Per-Application Rate Limiting:**

```csharp
// Middleware with distributed cache
public class ApplicationRateLimitingMiddleware
{
    private readonly IDistributedCache _cache;

    public async Task InvokeAsync(HttpContext context)
    {
        var applicationId = context.Items["ApplicationId"] as Guid?;
        if (!applicationId.HasValue)
        {
            await _next(context);
            return;
        }

        var application = context.Items["Application"] as Application;
        var cacheKey = $"rate_limit:{applicationId}:{DateTime.UtcNow:yyyyMMddHHmm}";

        var currentCount = await _cache.GetStringAsync(cacheKey);
        var count = int.Parse(currentCount ?? "0");

        if (count >= application.MaxRequestsPerMinute)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                limit = application.MaxRequestsPerMinute,
                retryAfter = 60
            });
            return;
        }

        await _cache.SetStringAsync(
            cacheKey,
            (count + 1).ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });

        await _next(context);
    }
}
```

### 3.4 Application Impersonation Prevention

**Strategies:**

1. **IP Whitelisting** (Optional):
```csharp
public class Application : BaseEntity
{
    private readonly List<string> _allowedIpAddresses = new();
    public IReadOnlyCollection<string> AllowedIpAddresses => _allowedIpAddresses.AsReadOnly();

    public void AddAllowedIpAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out _))
            throw new DomainException("Invalid IP address format.");

        _allowedIpAddresses.Add(ipAddress);
    }

    public bool IsIpAddressAllowed(string ipAddress)
    {
        // If no IPs are configured, allow all
        if (!_allowedIpAddresses.Any())
            return true;

        return _allowedIpAddresses.Contains(ipAddress);
    }
}
```

2. **Request Signature Verification**:
```csharp
// For critical operations, require HMAC signature
// X-Signature: HMAC-SHA256(request_body, secret_code)
public bool VerifyRequestSignature(string signature, string requestBody, string secretCode)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretCode));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
    var computedSignature = Convert.ToBase64String(hash);

    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(signature),
        Encoding.UTF8.GetBytes(computedSignature));
}
```

3. **Audit Logging**:
```csharp
public class ApplicationAccessLog : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public string IpAddress { get; set; }
    public string Endpoint { get; set; }
    public string Method { get; set; }
    public int StatusCode { get; set; }
    public string? UserId { get; set; }
    public DateTime AccessedAt { get; set; }
    public string? UserAgent { get; set; }
}
```

### 3.5 Security Best Practices Checklist

- [x] API Keys are hashed (SHA256) before storage
- [x] Secret Codes are encrypted (AES) before storage
- [x] Use constant-time comparison for hash verification
- [x] Implement rate limiting per application
- [x] Support IP whitelisting (optional)
- [x] Audit all authentication attempts
- [x] Lock applications after repeated failures
- [x] Rotate keys periodically (provide rotation endpoints)
- [x] Use HTTPS only (enforce in middleware)
- [x] Validate token audience (aud claim) matches application
- [x] Implement token revocation mechanism
- [x] Log security events for monitoring

---

## 4. Data Model & Database Schema

### 4.1 New Tables

**Applications Table:**
```sql
CREATE TABLE Applications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationName NVARCHAR(200) NOT NULL,
    ApplicationCode NVARCHAR(50) NOT NULL UNIQUE,
    SecretCodeHash NVARCHAR(500) NOT NULL,
    ApiKeyHash NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    RequireEmailConfirmation BIT NOT NULL DEFAULT 0,

    -- Settings (can be JSON column or separate table)
    TokenExpirationMinutes INT NOT NULL DEFAULT 60,
    RefreshTokenExpirationDays INT NOT NULL DEFAULT 7,
    AllowMultipleSessions BIT NOT NULL DEFAULT 1,
    EnableTwoFactorAuth BIT NOT NULL DEFAULT 0,
    CallbackUrl NVARCHAR(500) NULL,
    WebhookUrl NVARCHAR(500) NULL,

    -- Rate Limiting
    MaxRequestsPerMinute INT NOT NULL DEFAULT 100,
    MaxFailedAttemptsBeforeLock INT NOT NULL DEFAULT 5,
    LockedUntil DATETIME2 NULL,

    -- Audit
    LastAccessedAt DATETIME2 NULL,
    TotalRequestCount BIGINT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,

    INDEX IX_Applications_ApplicationCode (ApplicationCode),
    INDEX IX_Applications_IsActive (IsActive)
);
```

**UserApplications Table:**
```sql
CREATE TABLE UserApplications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    ApplicationSpecificUserId NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastAccessedAt DATETIME2 NULL,
    AccessCount BIGINT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_UserApplications_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserApplications_Applications FOREIGN KEY (ApplicationId)
        REFERENCES Applications(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_UserApplications_User_Application UNIQUE (UserId, ApplicationId),
    INDEX IX_UserApplications_UserId (UserId),
    INDEX IX_UserApplications_ApplicationId (ApplicationId),
    INDEX IX_UserApplications_IsActive (IsActive)
);
```

**ApplicationRoles Table:**
```sql
CREATE TABLE ApplicationRoles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_ApplicationRoles_Applications FOREIGN KEY (ApplicationId)
        REFERENCES Applications(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_ApplicationRoles_Application_Name UNIQUE (ApplicationId, Name),
    INDEX IX_ApplicationRoles_ApplicationId (ApplicationId),
    INDEX IX_ApplicationRoles_IsActive (IsActive)
);
```

**ApplicationPermissions Table:**
```sql
CREATE TABLE ApplicationPermissions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Resource NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_ApplicationPermissions_Applications FOREIGN KEY (ApplicationId)
        REFERENCES Applications(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_ApplicationPermissions_Application_Resource_Action
        UNIQUE (ApplicationId, Resource, Action),
    INDEX IX_ApplicationPermissions_ApplicationId (ApplicationId),
    INDEX IX_ApplicationPermissions_Resource_Action (Resource, Action),
    INDEX IX_ApplicationPermissions_IsActive (IsActive)
);
```

**ApplicationRolePermissions Table:**
```sql
CREATE TABLE ApplicationRolePermissions (
    ApplicationRoleId UNIQUEIDENTIFIER NOT NULL,
    ApplicationPermissionId UNIQUEIDENTIFIER NOT NULL,

    PRIMARY KEY (ApplicationRoleId, ApplicationPermissionId),

    CONSTRAINT FK_ApplicationRolePermissions_ApplicationRoles
        FOREIGN KEY (ApplicationRoleId)
        REFERENCES ApplicationRoles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ApplicationRolePermissions_ApplicationPermissions
        FOREIGN KEY (ApplicationPermissionId)
        REFERENCES ApplicationPermissions(Id) ON DELETE NO ACTION,

    INDEX IX_ApplicationRolePermissions_ApplicationRoleId (ApplicationRoleId),
    INDEX IX_ApplicationRolePermissions_ApplicationPermissionId (ApplicationPermissionId)
);
```

**UserApplicationRoles Table:**
```sql
CREATE TABLE UserApplicationRoles (
    UserApplicationId UNIQUEIDENTIFIER NOT NULL,
    ApplicationRoleId UNIQUEIDENTIFIER NOT NULL,

    PRIMARY KEY (UserApplicationId, ApplicationRoleId),

    CONSTRAINT FK_UserApplicationRoles_UserApplications
        FOREIGN KEY (UserApplicationId)
        REFERENCES UserApplications(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserApplicationRoles_ApplicationRoles
        FOREIGN KEY (ApplicationRoleId)
        REFERENCES ApplicationRoles(Id) ON DELETE NO ACTION,

    INDEX IX_UserApplicationRoles_UserApplicationId (UserApplicationId),
    INDEX IX_UserApplicationRoles_ApplicationRoleId (ApplicationRoleId)
);
```

**ApplicationAccessLogs Table (Optional for Audit):**
```sql
CREATE TABLE ApplicationAccessLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NULL,
    IpAddress NVARCHAR(50) NOT NULL,
    Endpoint NVARCHAR(500) NOT NULL,
    Method NVARCHAR(10) NOT NULL,
    StatusCode INT NOT NULL,
    AccessedAt DATETIME2 NOT NULL,
    UserAgent NVARCHAR(500) NULL,

    CONSTRAINT FK_ApplicationAccessLogs_Applications
        FOREIGN KEY (ApplicationId)
        REFERENCES Applications(Id) ON DELETE CASCADE,

    INDEX IX_ApplicationAccessLogs_ApplicationId_AccessedAt (ApplicationId, AccessedAt DESC),
    INDEX IX_ApplicationAccessLogs_UserId (UserId),
    INDEX IX_ApplicationAccessLogs_IpAddress (IpAddress)
);
```

**ApplicationAllowedIpAddresses Table (Optional):**
```sql
CREATE TABLE ApplicationAllowedIpAddresses (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    IpAddress NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,

    CONSTRAINT FK_ApplicationAllowedIpAddresses_Applications
        FOREIGN KEY (ApplicationId)
        REFERENCES Applications(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_ApplicationAllowedIpAddresses_Application_IpAddress
        UNIQUE (ApplicationId, IpAddress),
    INDEX IX_ApplicationAllowedIpAddresses_ApplicationId (ApplicationId)
);
```

### 4.2 Updated Entity Relationship Diagram

```
┌──────────────┐
│     User     │
└───────┬──────┘
        │
        │ 1:N
        ▼
┌─────────────────────┐        N:M         ┌────────────────────┐
│  UserApplication    │◄─────────────────►│  ApplicationRole   │
│  (Join Entity)      │  UserAppRoles      │                    │
└──────┬──────────────┘                    └─────────┬──────────┘
       │                                             │
       │ N:1                                         │ N:1
       ▼                                             ▼
┌──────────────────┐                        ┌───────────────────────┐
│   Application    │                        │ ApplicationPermission │
│  (Aggregate Root)│                        │                       │
└──────┬───────────┘                        └───────────────────────┘
       │                                             ▲
       │ 1:N                                         │
       │                                             │ N:M
       └─────────────────────────────────────────────┘
                    ApplicationRolePermissions
```

### 4.3 Key Indexes for Performance

```sql
-- For application lookup by code
CREATE INDEX IX_Applications_ApplicationCode ON Applications(ApplicationCode);

-- For user-application queries
CREATE INDEX IX_UserApplications_UserId_ApplicationId ON UserApplications(UserId, ApplicationId);
CREATE INDEX IX_UserApplications_ApplicationId_IsActive ON UserApplications(ApplicationId, IsActive);

-- For role lookups
CREATE INDEX IX_ApplicationRoles_ApplicationId_IsActive ON ApplicationRoles(ApplicationId, IsActive);

-- For permission checks
CREATE INDEX IX_ApplicationPermissions_ApplicationId_Resource ON ApplicationPermissions(ApplicationId, Resource);

-- For audit queries
CREATE INDEX IX_ApplicationAccessLogs_ApplicationId_AccessedAt ON ApplicationAccessLogs(ApplicationId, AccessedAt DESC);
CREATE INDEX IX_ApplicationAccessLogs_UserId_AccessedAt ON ApplicationAccessLogs(UserId, AccessedAt DESC);

-- For rate limiting queries (if using DB instead of cache)
CREATE INDEX IX_Applications_LastAccessedAt ON Applications(LastAccessedAt DESC);
```

### 4.4 Soft Deletes

**Recommendation:** Implement soft deletes for Applications to maintain audit trail.

```csharp
public abstract class BaseEntity
{
    // ... existing properties
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}

// Global query filter in DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Application>()
        .HasQueryFilter(a => !a.IsDeleted);

    // Apply to other entities as needed
}
```

---

## 5. API Design

### 5.1 Application Management Endpoints

**Base Path:** `/api/v1/applications`

#### 5.1.1 Register Application
```
POST /api/v1/applications/register

Request Body:
{
  "applicationName": "My External App",
  "applicationCode": "MYAPP001",
  "requireEmailConfirmation": false,
  "settings": {
    "tokenExpirationMinutes": 60,
    "refreshTokenExpirationDays": 7,
    "allowMultipleSessions": true,
    "enableTwoFactorAuth": false,
    "callbackUrl": "https://myapp.com/callback",
    "webhookUrl": "https://myapp.com/webhook"
  },
  "rateLimiting": {
    "maxRequestsPerMinute": 100,
    "maxFailedAttemptsBeforeLock": 5
  }
}

Response (201 Created):
{
  "applicationId": "guid",
  "applicationName": "My External App",
  "applicationCode": "MYAPP001",
  "apiKey": "generated-api-key-here",
  "secretCode": "generated-secret-code-here",
  "isActive": true,
  "createdAt": "2024-11-08T10:00:00Z",
  "warning": "Store the apiKey and secretCode securely. They will not be shown again."
}
```

#### 5.1.2 Rotate API Key
```
POST /api/v1/applications/{applicationId}/rotate-api-key

Headers:
  X-Application-Code: MYAPP001
  X-Secret-Code: current-secret-code

Response (200 OK):
{
  "applicationId": "guid",
  "newApiKey": "new-generated-api-key",
  "rotatedAt": "2024-11-08T10:00:00Z",
  "warning": "Update your application with the new API key. The old key will be invalidated."
}
```

#### 5.1.3 Rotate Secret Code
```
POST /api/v1/applications/{applicationId}/rotate-secret-code

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: current-api-key
  X-Secret-Code: current-secret-code

Response (200 OK):
{
  "applicationId": "guid",
  "newSecretCode": "new-generated-secret-code",
  "rotatedAt": "2024-11-08T10:00:00Z"
}
```

#### 5.1.4 Update Application Settings
```
PATCH /api/v1/applications/{applicationId}/settings

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "tokenExpirationMinutes": 120,
  "allowMultipleSessions": false
}

Response (200 OK):
{
  "applicationId": "guid",
  "settings": { ... },
  "updatedAt": "2024-11-08T10:00:00Z"
}
```

#### 5.1.5 Get Application Details
```
GET /api/v1/applications/{applicationId}

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Response (200 OK):
{
  "applicationId": "guid",
  "applicationName": "My External App",
  "applicationCode": "MYAPP001",
  "isActive": true,
  "settings": { ... },
  "statistics": {
    "totalUsers": 150,
    "activeUsers": 120,
    "totalRequests": 50000,
    "lastAccessedAt": "2024-11-08T09:00:00Z"
  }
}
```

### 5.2 User-Application Management Endpoints

**Base Path:** `/api/v1/applications/{applicationId}/users`

#### 5.2.1 Register User to Application
```
POST /api/v1/applications/{applicationId}/users

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!",
  "applicationSpecificUserId": "ext-user-123", // Optional
  "roleIds": ["role-guid-1", "role-guid-2"]
}

Response (201 Created):
{
  "userId": "guid",
  "userApplicationId": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "applicationSpecificUserId": "ext-user-123",
  "roles": ["Admin", "User"],
  "isActive": true,
  "createdAt": "2024-11-08T10:00:00Z"
}
```

#### 5.2.2 Add Existing User to Application
```
POST /api/v1/applications/{applicationId}/users/{userId}/add

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "applicationSpecificUserId": "ext-user-456",
  "roleIds": ["role-guid-1"]
}

Response (200 OK):
{
  "userApplicationId": "guid",
  "userId": "guid",
  "message": "User successfully added to application"
}
```

#### 5.2.3 Update User Roles in Application
```
PATCH /api/v1/applications/{applicationId}/users/{userId}/roles

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "roleIds": ["role-guid-1", "role-guid-3"]
}

Response (200 OK):
{
  "userId": "guid",
  "roles": ["Admin", "Editor"],
  "updatedAt": "2024-11-08T10:00:00Z"
}
```

#### 5.2.4 Deactivate User in Application
```
POST /api/v1/applications/{applicationId}/users/{userId}/deactivate

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Response (200 OK):
{
  "userId": "guid",
  "isActive": false,
  "deactivatedAt": "2024-11-08T10:00:00Z"
}
```

### 5.3 Authentication Endpoints

**Base Path:** `/api/v1/auth`

#### 5.3.1 Login (Single Application)
```
POST /api/v1/auth/login

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "email": "user@example.com",
  "password": "password123"
}

Response (200 OK):
{
  "token": "jwt-token",
  "refreshToken": "refresh-token",
  "expiresAt": "2024-11-08T11:00:00Z",
  "user": {
    "id": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "user@example.com"
  },
  "application": {
    "id": "guid",
    "name": "My External App",
    "code": "MYAPP001"
  },
  "roles": ["Admin", "User"],
  "permissions": [
    { "resource": "users", "action": "read" },
    { "resource": "users", "action": "write" }
  ]
}
```

#### 5.3.2 Login (Multiple Applications)
```
POST /api/v1/auth/multi-app-login

Headers:
  Content-Type: application/json

Request Body:
{
  "email": "user@example.com",
  "password": "password123",
  "applications": [
    {
      "applicationCode": "APP001",
      "apiKey": "api-key-1"
    },
    {
      "applicationCode": "APP002",
      "apiKey": "api-key-2"
    }
  ]
}

Response (200 OK):
{
  "applications": [
    {
      "applicationCode": "APP001",
      "applicationName": "App One",
      "token": "jwt-for-app1",
      "refreshToken": "refresh-token-1",
      "expiresAt": "2024-11-08T11:00:00Z",
      "roles": ["Admin"],
      "permissions": [...]
    },
    {
      "applicationCode": "APP002",
      "applicationName": "App Two",
      "token": "jwt-for-app2",
      "refreshToken": "refresh-token-2",
      "expiresAt": "2024-11-08T11:00:00Z",
      "roles": ["User"],
      "permissions": [...]
    }
  ],
  "user": {
    "id": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "user@example.com"
  }
}
```

#### 5.3.3 Refresh Token
```
POST /api/v1/auth/refresh

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "refreshToken": "refresh-token"
}

Response (200 OK):
{
  "token": "new-jwt-token",
  "refreshToken": "new-refresh-token",
  "expiresAt": "2024-11-08T12:00:00Z"
}
```

#### 5.3.4 Validate Token
```
POST /api/v1/auth/validate

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "token": "jwt-token"
}

Response (200 OK):
{
  "isValid": true,
  "userId": "guid",
  "applicationId": "guid",
  "expiresAt": "2024-11-08T11:00:00Z",
  "roles": ["Admin"],
  "permissions": [...]
}
```

### 5.4 Role & Permission Management Endpoints

**Base Path:** `/api/v1/applications/{applicationId}/roles`

#### 5.4.1 Create Role
```
POST /api/v1/applications/{applicationId}/roles

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "name": "Editor",
  "description": "Can edit content",
  "permissionIds": ["perm-guid-1", "perm-guid-2"]
}

Response (201 Created):
{
  "roleId": "guid",
  "name": "Editor",
  "description": "Can edit content",
  "permissions": [...],
  "createdAt": "2024-11-08T10:00:00Z"
}
```

#### 5.4.2 Create Permission
```
POST /api/v1/applications/{applicationId}/permissions

Headers:
  X-Application-Code: MYAPP001
  X-API-Key: api-key

Request Body:
{
  "name": "Edit Users",
  "description": "Allows editing user profiles",
  "resource": "users",
  "action": "write"
}

Response (201 Created):
{
  "permissionId": "guid",
  "name": "Edit Users",
  "resource": "users",
  "action": "write",
  "createdAt": "2024-11-08T10:00:00Z"
}
```

### 5.5 How External Apps Send Credentials

**Recommended Approach: Custom Headers**

```http
POST /api/v1/auth/login HTTP/1.1
Host: auth.example.com
X-Application-Code: MYAPP001
X-API-Key: your-api-key-here
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password"
}
```

**Alternative Approaches:**

1. **Basic Authentication for Application Credentials:**
```http
Authorization: Basic base64(ApplicationCode:APIKey)
```

2. **Custom Scheme:**
```http
Authorization: AppAuth ApplicationCode="MYAPP001", APIKey="your-api-key"
```

3. **Query Parameters (NOT RECOMMENDED for production):**
```http
POST /api/v1/auth/login?app_code=MYAPP001&api_key=key
```

**Recommendation:** Use custom headers (`X-Application-Code` and `X-API-Key`) as they:
- Keep credentials out of URL
- Are easy to implement
- Don't conflict with user authentication headers
- Can be easily validated in middleware

---

## 6. Implementation Strategy by Layer

### 6.1 Domain Layer

**Path:** `/src/GenericAuth.Domain/`

**New Files to Create:**

```
Domain/
├── Entities/
│   ├── Application.cs                    [NEW]
│   ├── UserApplication.cs                [NEW]
│   ├── ApplicationRole.cs                [NEW]
│   ├── ApplicationPermission.cs          [NEW]
│   ├── ApplicationRolePermission.cs      [NEW]
│   ├── UserApplicationRole.cs            [NEW]
│   ├── ApplicationAccessLog.cs           [NEW]
│   └── User.cs                           [MODIFY - Add UserApplications navigation]
├── ValueObjects/
│   ├── ApplicationCode.cs                [NEW]
│   └── ApplicationSettings.cs            [NEW]
├── Events/
│   ├── ApplicationRegisteredEvent.cs     [NEW]
│   ├── UserRegisteredToApplicationEvent.cs [NEW]
│   ├── ApplicationValidatedEvent.cs      [NEW]
│   ├── ApiKeyRotatedEvent.cs             [NEW]
│   ├── SecretCodeRotatedEvent.cs         [NEW]
│   ├── ApplicationAccessGrantedEvent.cs  [NEW]
│   └── ApplicationAccessDeniedEvent.cs   [NEW]
├── Services/
│   └── IApplicationSecurityService.cs    [NEW]
└── Exceptions/
    └── ApplicationSecurityException.cs   [NEW]
```

**Key Implementation Points:**

1. **Keep Application as Aggregate Root**: Don't expose internal collections, use methods
2. **Value Objects are Immutable**: ApplicationCode and ApplicationSettings
3. **Rich Domain Events**: Capture all significant state changes
4. **Domain Service for Security**: IApplicationSecurityService for encryption/hashing

### 6.2 Application Layer

**Path:** `/src/GenericAuth.Application/`

**New Files to Create:**

```
Application/
├── Features/
│   ├── Applications/
│   │   ├── Commands/
│   │   │   ├── RegisterApplication/
│   │   │   │   ├── RegisterApplicationCommand.cs
│   │   │   │   ├── RegisterApplicationCommandHandler.cs
│   │   │   │   └── RegisterApplicationCommandValidator.cs
│   │   │   ├── RotateApiKey/
│   │   │   │   ├── RotateApiKeyCommand.cs
│   │   │   │   └── RotateApiKeyCommandHandler.cs
│   │   │   ├── RotateSecretCode/
│   │   │   │   ├── RotateSecretCodeCommand.cs
│   │   │   │   └── RotateSecretCodeCommandHandler.cs
│   │   │   ├── UpdateApplicationSettings/
│   │   │   │   ├── UpdateApplicationSettingsCommand.cs
│   │   │   │   └── UpdateApplicationSettingsCommandHandler.cs
│   │   │   └── DeactivateApplication/
│   │   │       ├── DeactivateApplicationCommand.cs
│   │   │       └── DeactivateApplicationCommandHandler.cs
│   │   └── Queries/
│   │       ├── GetApplication/
│   │       │   ├── GetApplicationQuery.cs
│   │       │   └── GetApplicationQueryHandler.cs
│   │       └── GetApplications/
│   │           ├── GetApplicationsQuery.cs
│   │           └── GetApplicationsQueryHandler.cs
│   ├── UserApplications/
│   │   ├── Commands/
│   │   │   ├── RegisterUserToApplication/
│   │   │   │   ├── RegisterUserToApplicationCommand.cs
│   │   │   │   ├── RegisterUserToApplicationCommandHandler.cs
│   │   │   │   └── RegisterUserToApplicationCommandValidator.cs
│   │   │   ├── AddExistingUserToApplication/
│   │   │   │   ├── AddExistingUserToApplicationCommand.cs
│   │   │   │   └── AddExistingUserToApplicationCommandHandler.cs
│   │   │   ├── UpdateUserRolesInApplication/
│   │   │   │   ├── UpdateUserRolesInApplicationCommand.cs
│   │   │   │   └── UpdateUserRolesInApplicationCommandHandler.cs
│   │   │   └── DeactivateUserInApplication/
│   │   │       ├── DeactivateUserInApplicationCommand.cs
│   │   │       └── DeactivateUserInApplicationCommandHandler.cs
│   │   └── Queries/
│   │       ├── GetUserApplications/
│   │       │   ├── GetUserApplicationsQuery.cs
│   │       │   └── GetUserApplicationsQueryHandler.cs
│   │       └── GetApplicationUsers/
│   │           ├── GetApplicationUsersQuery.cs
│   │           └── GetApplicationUsersQueryHandler.cs
│   ├── ApplicationRoles/
│   │   ├── Commands/
│   │   │   ├── CreateApplicationRole/
│   │   │   └── UpdateApplicationRole/
│   │   └── Queries/
│   │       └── GetApplicationRoles/
│   ├── ApplicationPermissions/
│   │   ├── Commands/
│   │   │   ├── CreateApplicationPermission/
│   │   │   └── UpdateApplicationPermission/
│   │   └── Queries/
│   │       └── GetApplicationPermissions/
│   └── Authentication/
│       ├── Commands/
│       │   ├── Login/
│       │   │   ├── LoginCommand.cs              [MODIFY]
│       │   │   └── LoginCommandHandler.cs       [MODIFY]
│       │   ├── MultiAppLogin/
│       │   │   ├── MultiAppLoginCommand.cs      [NEW]
│       │   │   └── MultiAppLoginCommandHandler.cs [NEW]
│       │   ├── RefreshToken/
│       │   │   ├── RefreshTokenCommand.cs       [MODIFY]
│       │   │   └── RefreshTokenCommandHandler.cs [MODIFY]
│       │   └── ValidateToken/
│       │       ├── ValidateTokenCommand.cs      [NEW]
│       │       └── ValidateTokenCommandHandler.cs [NEW]
│       └── Queries/
│           └── ValidateApplicationCredentials/
│               ├── ValidateApplicationCredentialsQuery.cs [NEW]
│               └── ValidateApplicationCredentialsQueryHandler.cs [NEW]
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs          [MODIFY - Add DbSets]
│   │   ├── IJwtTokenGenerator.cs             [MODIFY - Add application context]
│   │   └── IApplicationContext.cs            [NEW - Current application context]
│   ├── Behaviors/
│   │   └── ApplicationValidationBehavior.cs  [NEW - MediatR pipeline]
│   └── Models/
│       ├── ApplicationDto.cs                 [NEW]
│       ├── UserApplicationDto.cs             [NEW]
│       ├── ApplicationRoleDto.cs             [NEW]
│       └── ApplicationPermissionDto.cs       [NEW]
└── DependencyInjection.cs                    [MODIFY]
```

**Example: RegisterApplicationCommandHandler**

```csharp
public class RegisterApplicationCommandHandler
    : IRequestHandler<RegisterApplicationCommand, Result<RegisterApplicationResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationSecurityService _securityService;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<RegisterApplicationResponse>> Handle(
        RegisterApplicationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if application code already exists
        var exists = await _context.Applications
            .AnyAsync(a => a.ApplicationCode == ApplicationCode.Create(request.ApplicationCode),
                cancellationToken);

        if (exists)
            return Result<RegisterApplicationResponse>.Failure(
                "Application with this code already exists.");

        // 2. Generate API Key and Secret Code
        var apiKey = _securityService.GenerateApiKey();
        var secretCode = _securityService.GenerateSecretCode();

        // 3. Create application
        var application = Application.Create(
            request.ApplicationName,
            request.ApplicationCode,
            secretCode,
            apiKey,
            _securityService);

        // 4. Set settings if provided
        if (request.Settings != null)
        {
            var settings = ApplicationSettings.Create(
                request.Settings.TokenExpirationMinutes,
                request.Settings.RefreshTokenExpirationDays,
                request.Settings.AllowMultipleSessions,
                request.Settings.EnableTwoFactorAuth,
                request.Settings.CallbackUrl,
                request.Settings.WebhookUrl);

            application.UpdateSettings(settings);
        }

        // 5. Add to context
        _context.Applications.Add(application);

        // 6. Save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return response with plain-text credentials (only time they're visible)
        var response = new RegisterApplicationResponse(
            ApplicationId: application.Id,
            ApplicationName: application.ApplicationName,
            ApplicationCode: application.ApplicationCode.Value,
            ApiKey: apiKey, // Plain text
            SecretCode: secretCode, // Plain text
            IsActive: application.IsActive,
            CreatedAt: application.CreatedAt);

        return Result<RegisterApplicationResponse>.Success(response);
    }
}
```

**Example: Modified LoginCommandHandler**

See Section 2.3 above for the detailed implementation.

### 6.3 Infrastructure Layer

**Path:** `/src/GenericAuth.Infrastructure/`

**New Files to Create:**

```
Infrastructure/
├── Persistence/
│   ├── Configurations/
│   │   ├── ApplicationConfiguration.cs       [NEW]
│   │   ├── UserApplicationConfiguration.cs   [NEW]
│   │   ├── ApplicationRoleConfiguration.cs   [NEW]
│   │   ├── ApplicationPermissionConfiguration.cs [NEW]
│   │   ├── ApplicationRolePermissionConfiguration.cs [NEW]
│   │   └── UserApplicationRoleConfiguration.cs [NEW]
│   ├── Migrations/
│   │   └── AddMultiTenantSupport.cs          [NEW - EF Migration]
│   └── ApplicationDbContext.cs               [MODIFY - Add DbSets]
├── Security/
│   └── ApplicationSecurityService.cs         [NEW]
├── Identity/
│   └── JwtTokenGenerator.cs                  [MODIFY - Add application claims]
└── DependencyInjection.cs                    [MODIFY]
```

**Example: ApplicationConfiguration**

```csharp
public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.ApplicationName)
            .IsRequired()
            .HasMaxLength(200);

        // ApplicationCode value object
        builder.OwnsOne(a => a.ApplicationCode, code =>
        {
            code.Property(c => c.Value)
                .HasColumnName("ApplicationCode")
                .IsRequired()
                .HasMaxLength(50);

            code.HasIndex(c => c.Value)
                .IsUnique();
        });

        builder.Property(a => a.SecretCodeHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ApiKeyHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ApplicationSettings value object (as JSON or owned entity)
        builder.OwnsOne(a => a.Settings, settings =>
        {
            settings.Property(s => s.TokenExpirationMinutes)
                .HasColumnName("TokenExpirationMinutes")
                .HasDefaultValue(60);

            settings.Property(s => s.RefreshTokenExpirationDays)
                .HasColumnName("RefreshTokenExpirationDays")
                .HasDefaultValue(7);

            settings.Property(s => s.AllowMultipleSessions)
                .HasColumnName("AllowMultipleSessions")
                .HasDefaultValue(true);

            settings.Property(s => s.EnableTwoFactorAuth)
                .HasColumnName("EnableTwoFactorAuth")
                .HasDefaultValue(false);

            settings.Property(s => s.CallbackUrl)
                .HasColumnName("CallbackUrl")
                .HasMaxLength(500);

            settings.Property(s => s.WebhookUrl)
                .HasColumnName("WebhookUrl")
                .HasMaxLength(500);
        });

        builder.Property(a => a.MaxRequestsPerMinute)
            .HasDefaultValue(100);

        builder.Property(a => a.MaxFailedAttemptsBeforeLock)
            .HasDefaultValue(5);

        builder.Property(a => a.LockedUntil);

        builder.Property(a => a.LastAccessedAt);

        builder.Property(a => a.TotalRequestCount)
            .HasDefaultValue(0);

        // Audit fields
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.CreatedBy).HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);

        // Soft delete
        builder.Property(a => a.IsDeleted).HasDefaultValue(false);
        builder.Property(a => a.DeletedAt);
        builder.Property(a => a.DeletedBy).HasMaxLength(100);

        // Relationships
        builder.HasMany(a => a.UserApplications)
            .WithOne(ua => ua.Application)
            .HasForeignKey(ua => ua.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Roles)
            .WithOne(r => r.Application)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Permissions)
            .WithOne(p => p.Application)
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);

        // Global query filter for soft deletes
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
```

**Example: Modified JwtTokenGenerator**

```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    public string GenerateToken(
        User user,
        Application application,
        IEnumerable<string> roles,
        IEnumerable<PermissionDto> permissions)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Application context claims
            new Claim("app_id", application.Id.ToString()),
            new Claim("app_code", application.ApplicationCode.Value),
            new Claim("app_name", application.ApplicationName),
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions (as JSON array or individual claims)
        var permissionsJson = JsonSerializer.Serialize(permissions);
        claims.Add(new Claim("permissions", permissionsJson));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: application.ApplicationCode.Value, // Audience is the application
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(application.Settings.TokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Example: ApplicationSecurityService**

See Section 3 above for the complete implementation.

### 6.4 API Layer

**Path:** `/src/GenericAuth.API/`

**New Files to Create:**

```
API/
├── Controllers/
│   ├── ApplicationsController.cs            [NEW]
│   ├── UserApplicationsController.cs        [NEW]
│   ├── ApplicationRolesController.cs        [NEW]
│   ├── ApplicationPermissionsController.cs  [NEW]
│   └── AuthenticationController.cs          [MODIFY]
├── Middleware/
│   ├── ApplicationAuthenticationMiddleware.cs [NEW]
│   ├── ApplicationRateLimitingMiddleware.cs   [NEW]
│   └── ApplicationAuditMiddleware.cs          [NEW]
├── Filters/
│   ├── RequireApplicationAttribute.cs       [NEW - Action filter]
│   └── RequireApplicationRoleAttribute.cs   [NEW - Authorization filter]
├── Services/
│   └── ApplicationContextService.cs         [NEW - Scoped service]
└── Program.cs                               [MODIFY - Register middleware]
```

**Example: ApplicationAuthenticationMiddleware**

See Section 2.3 above for implementation.

**Example: ApplicationsController**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.ApplicationId },
            result.Value);
    }

    [HttpGet("{id}")]
    [RequireApplication] // Custom filter that validates app credentials
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetApplicationQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("{id}/rotate-api-key")]
    [RequireSecretCode] // Custom filter that validates secret code
    [ProducesResponseType(typeof(RotateApiKeyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RotateApiKey(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RotateApiKeyCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
```

**Example: Modified Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// ... existing services

// Add distributed cache for rate limiting
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add application context service
builder.Services.AddScoped<IApplicationContext, ApplicationContextService>();

var app = builder.Build();

// ... existing middleware

// Add custom middleware (order matters!)
app.UseMiddleware<ApplicationAuthenticationMiddleware>();
app.UseMiddleware<ApplicationRateLimitingMiddleware>();
app.UseMiddleware<ApplicationAuditMiddleware>();

// ... rest of pipeline

app.Run();
```

---

## 7. Migration Strategy

### 7.1 Database Migration Approach

**Phased Migration Strategy:**

**Phase 1: Add New Tables (Non-Breaking)**
```csharp
public class AddApplicationTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create all new tables
        migrationBuilder.CreateTable(
            name: "Applications",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                ApplicationName = table.Column<string>(maxLength: 200, nullable: false),
                // ... all columns
            });

        // Create UserApplications table
        migrationBuilder.CreateTable(
            name: "UserApplications",
            columns: table => new
            {
                // ... columns
            });

        // ... other tables

        // Add indexes
        migrationBuilder.CreateIndex(
            name: "IX_Applications_ApplicationCode",
            table: "Applications",
            column: "ApplicationCode",
            unique: true);
    }
}
```

**Phase 2: Create Default Application**

To maintain backward compatibility, create a "default" application for existing users:

```csharp
public class SeedDefaultApplication : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Insert default application
        migrationBuilder.InsertData(
            table: "Applications",
            columns: new[] {
                "Id",
                "ApplicationName",
                "ApplicationCode",
                "SecretCodeHash",
                "ApiKeyHash",
                "IsActive",
                "TokenExpirationMinutes",
                "RefreshTokenExpirationDays",
                "AllowMultipleSessions",
                "EnableTwoFactorAuth",
                "MaxRequestsPerMinute",
                "MaxFailedAttemptsBeforeLock",
                "TotalRequestCount",
                "CreatedAt",
                "IsDeleted"
            },
            values: new object[] {
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "Default Application",
                "DEFAULT",
                "hashed-secret-code",
                "hashed-api-key",
                true,
                60,
                7,
                true,
                false,
                1000,
                5,
                0,
                DateTime.UtcNow,
                false
            });
    }
}
```

**Phase 3: Migrate Existing Users**

Create UserApplication records for all existing users linked to the default application:

```csharp
public class MigrateExistingUsersToDefaultApp : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQL to create UserApplication for each existing user
        migrationBuilder.Sql(@"
            INSERT INTO UserApplications (Id, UserId, ApplicationId, IsActive, AccessCount, CreatedAt, IsDeleted)
            SELECT
                NEWID(),
                u.Id,
                '00000000-0000-0000-0000-000000000001', -- Default app
                1,
                0,
                GETUTCDATE(),
                0
            FROM Users u
            WHERE NOT EXISTS (
                SELECT 1 FROM UserApplications ua
                WHERE ua.UserId = u.Id
                AND ua.ApplicationId = '00000000-0000-0000-0000-000000000001'
            )
        ");
    }
}
```

**Phase 4: Migrate Roles to Application-Scoped**

If you want to maintain existing global roles, copy them to the default application:

```csharp
public class MigrateRolesToApplicationRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Copy existing roles to ApplicationRoles for default app
        migrationBuilder.Sql(@"
            INSERT INTO ApplicationRoles (Id, ApplicationId, Name, Description, IsActive, CreatedAt, IsDeleted)
            SELECT
                NEWID(),
                '00000000-0000-0000-0000-000000000001',
                r.Name,
                r.Description,
                r.IsActive,
                r.CreatedAt,
                0
            FROM Roles r
        ");

        // Copy permissions
        migrationBuilder.Sql(@"
            INSERT INTO ApplicationPermissions (Id, ApplicationId, Name, Description, Resource, Action, IsActive, CreatedAt, IsDeleted)
            SELECT
                NEWID(),
                '00000000-0000-0000-0000-000000000001',
                p.Name,
                p.Description,
                p.Resource,
                p.Action,
                p.IsActive,
                p.CreatedAt,
                0
            FROM Permissions p
        ");

        // Create role-permission mappings
        // (More complex SQL needed here to map IDs)
    }
}
```

**Phase 5: Migrate User Roles**

```csharp
public class MigrateUserRolesToUserApplicationRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            -- This is complex and requires careful ID mapping
            -- You may want to do this in application code instead
        ");
    }
}
```

### 7.2 Application Code Migration

**Backward Compatibility Service:**

```csharp
// Infrastructure/Services/BackwardCompatibilityService.cs
public class BackwardCompatibilityService
{
    private static readonly Guid DefaultApplicationId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid GetDefaultApplicationId() => DefaultApplicationId;

    public async Task<Application> GetDefaultApplicationAsync(
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        return await context.Applications
            .FirstAsync(a => a.Id == DefaultApplicationId, cancellationToken);
    }
}
```

**Updated Login Handler (with backward compatibility):**

```csharp
public async Task<Result<LoginCommandResponse>> Handle(
    LoginCommand request,
    CancellationToken cancellationToken)
{
    // Try to get application context from middleware
    var applicationId = _httpContextAccessor.HttpContext?.Items["ApplicationId"] as Guid?;

    // If no application context, use default (backward compatibility)
    if (!applicationId.HasValue)
    {
        applicationId = _backwardCompatibilityService.GetDefaultApplicationId();
    }

    // Rest of the login logic...
}
```

### 7.3 Migration Checklist

- [ ] **Phase 1**: Deploy database migrations (add new tables)
- [ ] **Phase 2**: Seed default application
- [ ] **Phase 3**: Migrate existing users to default application
- [ ] **Phase 4**: Migrate roles and permissions (optional if keeping global roles)
- [ ] **Phase 5**: Test backward compatibility
- [ ] **Phase 6**: Update client applications to use ApplicationCode + APIKey
- [ ] **Phase 7**: Gradually deprecate default application usage
- [ ] **Phase 8**: (Optional) Remove global Roles/Permissions tables if fully migrated

### 7.4 Rollback Plan

**If migration fails:**

1. **Database Rollback**: Use EF Core migration rollback
```bash
dotnet ef database update PreviousMigration
```

2. **Feature Flag**: Use feature flags to toggle multi-tenant features
```csharp
if (_featureManager.IsEnabledAsync("MultiTenantAuth").Result)
{
    // New multi-tenant logic
}
else
{
    // Old logic
}
```

3. **Dual-Write Pattern**: Write to both old and new structures during transition
4. **Data Verification**: Compare old vs new data regularly during migration

---

## 8. Code Structure & Organization

### 8.1 Recommended Project Structure

```
GenericAuth/
├── src/
│   ├── GenericAuth.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── ValueObject.cs
│   │   │   └── IDomainEvent.cs
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Application.cs
│   │   │   ├── UserApplication.cs
│   │   │   ├── ApplicationRole.cs
│   │   │   ├── ApplicationPermission.cs
│   │   │   └── ... (other entities)
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   ├── Password.cs
│   │   │   ├── ApplicationCode.cs
│   │   │   └── ApplicationSettings.cs
│   │   ├── Events/
│   │   │   ├── ApplicationRegisteredEvent.cs
│   │   │   └── ... (other events)
│   │   ├── Services/
│   │   │   └── IApplicationSecurityService.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       └── ApplicationSecurityException.cs
│   │
│   ├── GenericAuth.Application/
│   │   ├── Features/
│   │   │   ├── Applications/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   ├── UserApplications/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   ├── ApplicationRoles/
│   │   │   └── Authentication/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Behaviors/
│   │   │   ├── Models/
│   │   │   └── Exceptions/
│   │   └── DependencyInjection.cs
│   │
│   ├── GenericAuth.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Security/
│   │   │   └── ApplicationSecurityService.cs
│   │   ├── Identity/
│   │   │   ├── JwtTokenGenerator.cs
│   │   │   └── PasswordHasher.cs
│   │   ├── Services/
│   │   │   └── DateTimeService.cs
│   │   └── DependencyInjection.cs
│   │
│   └── GenericAuth.API/
│       ├── Controllers/
│       │   ├── ApplicationsController.cs
│       │   ├── UserApplicationsController.cs
│       │   └── AuthenticationController.cs
│       ├── Middleware/
│       │   ├── ApplicationAuthenticationMiddleware.cs
│       │   ├── ApplicationRateLimitingMiddleware.cs
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Filters/
│       │   ├── RequireApplicationAttribute.cs
│       │   └── RequireApplicationRoleAttribute.cs
│       ├── Services/
│       │   └── ApplicationContextService.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── GenericAuth.Domain.UnitTests/
│   │   ├── Entities/
│   │   │   ├── ApplicationTests.cs
│   │   │   ├── UserApplicationTests.cs
│   │   │   └── UserTests.cs
│   │   └── ValueObjects/
│   │       ├── ApplicationCodeTests.cs
│   │       └── ApplicationSettingsTests.cs
│   ├── GenericAuth.Application.UnitTests/
│   │   ├── Applications/
│   │   │   └── RegisterApplicationCommandHandlerTests.cs
│   │   └── Authentication/
│   │       └── LoginCommandHandlerTests.cs
│   ├── GenericAuth.Infrastructure.IntegrationTests/
│   │   ├── Persistence/
│   │   └── Security/
│   └── GenericAuth.API.IntegrationTests/
│       ├── Controllers/
│       │   ├── ApplicationsControllerTests.cs
│       │   └── AuthenticationControllerTests.cs
│       └── Middleware/
│           └── ApplicationAuthenticationMiddlewareTests.cs
│
└── docs/
    ├── ARCHITECTURE.md
    ├── API_DOCUMENTATION.md
    ├── MULTI_TENANT_ARCHITECTURE_PLAN.md (this file)
    └── MIGRATION_GUIDE.md
```

### 8.2 Naming Conventions

**Entities:**
- PascalCase
- Singular nouns
- Examples: `Application`, `UserApplication`, `ApplicationRole`

**Value Objects:**
- PascalCase
- Descriptive names
- Examples: `ApplicationCode`, `ApplicationSettings`, `Email`

**Commands:**
- Verb + Noun + "Command"
- Examples: `RegisterApplicationCommand`, `RotateApiKeyCommand`

**Queries:**
- "Get" + Description + "Query"
- Examples: `GetApplicationQuery`, `GetApplicationsQuery`

**Handlers:**
- Command/Query name + "Handler"
- Examples: `RegisterApplicationCommandHandler`, `GetApplicationQueryHandler`

**DTOs:**
- Noun + "Dto"
- Examples: `ApplicationDto`, `UserApplicationDto`

**Events:**
- Past tense + "Event"
- Examples: `ApplicationRegisteredEvent`, `ApiKeyRotatedEvent`

### 8.3 Dependency Rules

```
API Layer
  ↓ depends on
Application Layer
  ↓ depends on
Domain Layer
  ↑ implemented by
Infrastructure Layer
```

**Key Rules:**
- Domain has NO dependencies (pure business logic)
- Application depends ONLY on Domain
- Infrastructure implements Domain & Application interfaces
- API depends on Application (not Infrastructure directly)
- Use Dependency Injection to wire everything in API layer

---

## 9. Potential Pitfalls & Considerations

### 9.1 Performance Concerns

**Problem 1: N+1 Queries**

When loading user with all applications, roles, and permissions:

```csharp
// BAD: Multiple round trips
var user = await _context.Users.FindAsync(userId);
foreach (var ua in user.UserApplications)
{
    var app = await _context.Applications.FindAsync(ua.ApplicationId);
    // N+1 problem
}

// GOOD: Single query with eager loading
var user = await _context.Users
    .Include(u => u.UserApplications)
        .ThenInclude(ua => ua.Application)
    .Include(u => u.UserApplications)
        .ThenInclude(ua => ua.UserApplicationRoles)
            .ThenInclude(uar => uar.ApplicationRole)
                .ThenInclude(ar => ar.RolePermissions)
                    .ThenInclude(rp => rp.ApplicationPermission)
    .FirstOrDefaultAsync(u => u.Id == userId);
```

**Solution:**
- Use projection with Dapper for read operations
- Implement specific read models
- Use caching for frequently accessed data

**Problem 2: Large Token Size**

Including many permissions in JWT can make tokens very large.

**Solutions:**
1. **Store permissions separately, reference by ID**
```json
{
  "permissions_hash": "abc123",
  // Look up actual permissions from cache/DB using hash
}
```

2. **Use permission groups/scopes instead of individual permissions**
```json
{
  "scopes": ["users:read", "users:write", "reports:read"]
}
```

3. **Paginate permissions** (load on-demand)

### 9.2 Security Pitfalls

**Pitfall 1: Storing API Keys in Plain Text**

- **Never** store API keys unhashed
- Use SHA256 minimum (bcrypt not needed since API keys are random)
- Use constant-time comparison to prevent timing attacks

**Pitfall 2: Logging Sensitive Data**

```csharp
// BAD
_logger.LogInformation("API Key: {ApiKey}", apiKey);

// GOOD
_logger.LogInformation("API Key rotation completed for Application {AppCode}", appCode);
```

**Pitfall 3: Insufficient Rate Limiting**

- Implement rate limiting at multiple levels:
  - Per application
  - Per user
  - Per IP address
  - Per endpoint

**Pitfall 4: Missing Token Validation**

Always validate:
- Token signature
- Expiration
- Issuer
- Audience (application code)
- User still has access to application
- Application is still active

### 9.3 Data Integrity Issues

**Issue 1: Orphaned Records**

When deleting an application, ensure cascading deletes are configured:

```csharp
// In ApplicationConfiguration
builder.HasMany(a => a.UserApplications)
    .WithOne(ua => ua.Application)
    .HasForeignKey(ua => ua.ApplicationId)
    .OnDelete(DeleteBehavior.Cascade); // Important!
```

**Issue 2: Inconsistent State**

Use transactions for multi-entity operations:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    application.RotateApiKey(newKey, _securityService);
    await _context.SaveChangesAsync();

    // Log the rotation
    await _auditService.LogKeyRotation(application.Id);

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Issue 3: Concurrent Modifications**

Use optimistic concurrency with row versioning:

```csharp
public abstract class BaseEntity
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

### 9.4 Scalability Considerations

**Consideration 1: Database Connection Pooling**

With many applications making requests:
- Configure appropriate connection pool size
- Use connection string parameters: `Min Pool Size=10;Max Pool Size=100`
- Monitor connection pool exhaustion

**Consideration 2: Caching Strategy**

Cache at multiple levels:

```csharp
// Application-level cache (rarely changes)
var application = await _cache.GetOrCreateAsync(
    $"app:{applicationCode}",
    async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.ApplicationCode == code);
    });

// User-application cache (changes more frequently)
var userApp = await _cache.GetOrCreateAsync(
    $"userapp:{userId}:{applicationId}",
    async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
        return await _context.UserApplications...;
    });
```

**Consideration 3: Database Indexing**

Ensure proper indexes exist (see Section 4.3).

**Consideration 4: Read Replicas**

For read-heavy workloads:
- Use Dapper with read replicas for queries
- Use EF Core with write database for commands
- Implement eventual consistency where acceptable

### 9.5 Development Workflow Pitfalls

**Pitfall 1: Not Testing with Multiple Applications**

Always test scenarios with:
- Single application
- Multiple applications for same user
- No applications (error cases)
- Inactive applications

**Pitfall 2: Forgetting Migration Rollback Plan**

Before each migration:
- Test rollback in dev environment
- Document data loss scenarios
- Have backup strategy

**Pitfall 3: Tight Coupling**

Avoid coupling application logic to specific applications:

```csharp
// BAD
if (application.ApplicationCode == "SPECIAL_APP")
{
    // Special logic
}

// GOOD
if (application.Settings.EnableSpecialFeature)
{
    // Feature-based logic
}
```

### 9.6 Operational Considerations

**Monitoring & Alerting:**

Track these metrics:
- Authentication success/failure rate per application
- API key rotation frequency
- Application lock events
- Rate limit hits
- Token generation time
- Database query performance

**Audit & Compliance:**

Ensure you log:
- All authentication attempts
- API key rotations
- Permission changes
- User-application association changes
- Application creation/deletion

**Disaster Recovery:**

- Regular backups of application credentials
- Document API key recovery process
- Have plan for bulk key rotation (in case of breach)

### 9.7 Common Questions & Answers

**Q: Should permissions be per-application or global?**

A: **Per-application** provides better tenant isolation. Each application defines its own permission model. This allows different applications to have completely different authorization structures.

**Q: How to handle a user logging into multiple applications simultaneously?**

A: Issue separate JWT tokens for each application (each with different `aud` claim). The client stores all tokens and uses the appropriate one for each service.

**Q: What if an application needs to revoke access immediately?**

A: Implement token revocation:
- Store active tokens in Redis with expiration
- Check revocation list on each request (or use short-lived tokens)
- Provide `/auth/revoke` endpoint

**Q: How to handle application deletion?**

A: Use soft deletes:
- Set `IsDeleted = true`
- Keep data for audit trail
- Prevent login but maintain historical data
- Hard delete after retention period (e.g., 90 days)

**Q: Should we support application hierarchies (parent/child apps)?**

A: Not in v1. Keep it simple. If needed later, add:
```csharp
public Guid? ParentApplicationId { get; set; }
public Application? ParentApplication { get; set; }
```

---

## 10. Next Steps & Implementation Plan

### Phase 1: Foundation (Week 1-2)
- [ ] Create domain entities (Application, UserApplication, etc.)
- [ ] Create value objects (ApplicationCode, ApplicationSettings)
- [ ] Create domain events
- [ ] Implement IApplicationSecurityService
- [ ] Write unit tests for domain logic

### Phase 2: Infrastructure (Week 2-3)
- [ ] Create EF Core configurations
- [ ] Implement ApplicationSecurityService
- [ ] Create database migrations
- [ ] Update ApplicationDbContext
- [ ] Seed default application
- [ ] Write integration tests for persistence

### Phase 3: Application Layer (Week 3-4)
- [ ] Create application registration command/handler
- [ ] Create user-application commands/handlers
- [ ] Update login command handler
- [ ] Create multi-app login handler
- [ ] Implement validation behaviors
- [ ] Write unit tests for handlers

### Phase 4: API Layer (Week 4-5)
- [ ] Create middleware (auth, rate limiting, audit)
- [ ] Create controllers
- [ ] Create custom filters/attributes
- [ ] Update Program.cs
- [ ] Write API integration tests

### Phase 5: Migration (Week 5-6)
- [ ] Run migrations in dev environment
- [ ] Migrate test data
- [ ] Test backward compatibility
- [ ] Document migration process
- [ ] Create rollback plan

### Phase 6: Testing & Refinement (Week 6-7)
- [ ] End-to-end testing
- [ ] Performance testing
- [ ] Security testing (penetration testing)
- [ ] Load testing
- [ ] Fix bugs and optimize

### Phase 7: Documentation & Deployment (Week 7-8)
- [ ] API documentation (Swagger/OpenAPI)
- [ ] Developer guide for external apps
- [ ] Admin guide for application management
- [ ] Deployment runbook
- [ ] Monitoring & alerting setup

---

## Conclusion

This architecture provides a robust, scalable, and secure foundation for multi-tenant authentication. Key benefits:

1. **Tenant Isolation**: Each application is completely isolated with its own roles/permissions
2. **Security**: API keys hashed, secret codes encrypted, comprehensive audit trail
3. **Scalability**: Designed for high throughput with caching and rate limiting
4. **Flexibility**: Applications can configure their own settings (token expiration, etc.)
5. **DDD Principles**: Strong domain model with clear aggregate boundaries
6. **Clean Architecture**: Proper separation of concerns across layers
7. **Backward Compatibility**: Migration path from existing single-tenant system

The implementation follows industry best practices for authentication systems and provides a solid foundation that can evolve as requirements change.

**Estimated Implementation Time:** 7-8 weeks for a complete implementation with thorough testing.

**Team Recommendation:**
- 2 Backend Developers
- 1 DevOps Engineer (for infrastructure & deployment)
- 1 QA Engineer (for comprehensive testing)

Good luck with the implementation! This is a solid architectural foundation for building a world-class multi-tenant authentication system.
