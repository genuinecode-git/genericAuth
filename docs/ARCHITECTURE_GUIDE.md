# GenericAuth Architecture Guide

This document provides a comprehensive overview of the GenericAuth system architecture, design patterns, and implementation details.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Clean Architecture Layers](#clean-architecture-layers)
3. [Multi-Tenant Architecture](#multi-tenant-architecture)
4. [Design Patterns](#design-patterns)
5. [Domain-Driven Design](#domain-driven-design)
6. [CQRS Implementation](#cqrs-implementation)
7. [Security Architecture](#security-architecture)
8. [API Versioning](#api-versioning)
9. [Data Flow](#data-flow)
10. [Technology Stack](#technology-stack)

---

## Architecture Overview

GenericAuth is built using **Clean Architecture** principles with **Domain-Driven Design (DDD)** and **CQRS** patterns. The architecture ensures:

- **Separation of Concerns**: Clear boundaries between layers
- **Testability**: Easy to test with minimal dependencies
- **Maintainability**: Changes in one layer don't affect others
- **Scalability**: Designed to scale horizontally
- **Multi-Tenancy**: Application-scoped isolation

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation Layer                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ Controllers │  │  Middleware │  │  API Versioning     │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP Requests
┌────────────────────────▼────────────────────────────────────┐
│                     Application Layer                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  Commands   │  │   Queries   │  │   Validators        │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │   MediatR   │  │ AutoMapper  │  │  FluentValidation   │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │ Business Logic
┌────────────────────────▼────────────────────────────────────┐
│                       Domain Layer                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  Entities   │  │Value Objects│  │  Domain Events      │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ Aggregates  │  │ Interfaces  │  │  Business Rules     │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │ Data Access
┌────────────────────────▼────────────────────────────────────┐
│                   Infrastructure Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  DbContext  │  │Repositories │  │  Identity Service   │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ Migrations  │  │   Seeding   │  │  External Services  │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │ Database
┌────────────────────────▼────────────────────────────────────┐
│                      SQL Server Database                     │
└──────────────────────────────────────────────────────────────┘
```

---

## Clean Architecture Layers

### Layer Dependencies

The dependency rule: **Dependencies point inward**

```
API Layer → Application Layer → Domain Layer ← Infrastructure Layer
```

- **Domain Layer**: No dependencies (pure business logic)
- **Application Layer**: Depends only on Domain
- **Infrastructure Layer**: Depends on Domain and Application
- **API Layer**: Depends on Application and Infrastructure

### 1. Domain Layer (`GenericAuth.Domain`)

**Purpose**: Contains all business entities, value objects, and domain logic.

**Contents:**
```
Domain/
├── Entities/
│   ├── User.cs                 # User aggregate root
│   ├── Application.cs          # Application aggregate root
│   ├── ApplicationRole.cs      # Application role entity
│   ├── Role.cs                 # System role entity
│   ├── Permission.cs           # Permission entity
│   ├── RefreshToken.cs         # Refresh token entity
│   ├── UserApplication.cs      # User-Application relationship
│   ├── UserRole.cs            # User-System Role relationship
│   ├── RolePermission.cs      # Role-Permission relationship
│   └── ApplicationRolePermission.cs
├── ValueObjects/
│   ├── Email.cs               # Email value object
│   └── PasswordHash.cs        # Password hash value object
├── Enums/
│   └── UserType.cs            # Regular, AuthAdmin
├── Events/
│   ├── UserCreatedEvent.cs
│   ├── UserAssignedToApplicationEvent.cs
│   └── RoleChangedEvent.cs
└── Common/
    └── BaseEntity.cs          # Base entity with Id, CreatedAt, UpdatedAt
```

**Key Principles:**
- Entities are rich domain models with business logic
- No infrastructure concerns (no EF, no repositories)
- Value objects are immutable
- Domain events capture business occurrences

**Example Entity:**
```csharp
public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public UserType UserType { get; private set; }
    public bool IsActive { get; private set; }

    // Collections
    public ICollection<UserApplication> UserApplications { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; private set; }

    // Business methods
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("User is already active.");
        IsActive = true;
    }

    public void AssignToApplication(Application application, ApplicationRole role)
    {
        if (UserType == UserType.AuthAdmin)
            throw new DomainException("Auth Admin users cannot be assigned to applications.");

        // Business logic here
    }
}
```

### 2. Application Layer (`GenericAuth.Application`)

**Purpose**: Orchestrates business workflows, handles use cases via CQRS.

**Contents:**
```
Application/
├── Features/
│   ├── Authentication/
│   │   ├── Commands/
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   └── LoginCommandValidator.cs
│   │   │   ├── Register/
│   │   │   ├── RefreshToken/
│   │   │   ├── Logout/
│   │   │   ├── ForgotPassword/
│   │   │   └── ResetPassword/
│   ├── Users/
│   │   ├── Commands/
│   │   │   ├── ActivateUser/
│   │   │   ├── DeactivateUser/
│   │   │   ├── UpdateUser/
│   │   │   ├── AssignRoleToUser/
│   │   │   └── RemoveRoleFromUser/
│   │   └── Queries/
│   │       ├── GetAllUsers/
│   │       └── GetUserById/
│   ├── Applications/
│   ├── ApplicationRoles/
│   ├── Roles/
│   └── UserApplications/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── IIdentityService.cs
│   │   ├── ITokenService.cs
│   │   └── IRepository<T>.cs
│   ├── Models/
│   │   ├── Result.cs          # Result pattern
│   │   └── PaginatedList.cs   # Pagination
│   └── Behaviors/
│       ├── ValidationBehavior.cs
│       └── LoggingBehavior.cs
└── DependencyInjection.cs
```

**Key Principles:**
- One command/query per use case
- Handlers contain workflow orchestration
- Validators ensure data integrity
- No direct database access (uses interfaces)

**Example Command:**
```csharp
// Command (request)
public record LoginCommand(
    string Email,
    string Password,
    Guid? ApplicationId) : IRequest<Result<LoginResponse>>;

// Handler (orchestration)
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == request.Email);

        if (user == null)
            return Result<LoginResponse>.Failure("Invalid email or password.");

        // 2. Verify password
        if (!_identityService.VerifyPassword(request.Password, user.PasswordHash.Value))
            return Result<LoginResponse>.Failure("Invalid email or password.");

        // 3. Check active status
        if (!user.IsActive)
            return Result<LoginResponse>.Failure("User account is inactive.");

        // 4. Generate tokens
        var tokens = await _tokenService.GenerateTokens(user, request.ApplicationId);

        // 5. Return success
        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            User = MapToDto(user)
        });
    }
}

// Validator (data integrity)
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
```

### 3. Infrastructure Layer (`GenericAuth.Infrastructure`)

**Purpose**: Implements interfaces defined in Application layer, handles external concerns.

**Contents:**
```
Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── ApplicationConfiguration.cs
│   │   └── ...
│   ├── Migrations/
│   ├── Repositories/
│   │   └── Repository<T>.cs
│   └── DatabaseSeeder.cs
├── Identity/
│   ├── IdentityService.cs     # Password hashing/verification
│   └── TokenService.cs        # JWT generation
├── Services/
│   └── DateTimeService.cs
└── DependencyInjection.cs
```

**Key Principles:**
- Implements Application layer interfaces
- Contains EF Core configurations
- Manages database migrations
- Handles external service integrations

**Example Service:**
```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public async Task<TokenResponse> GenerateTokens(User user, Guid? applicationId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email.Value),
            new("user_type", user.UserType.ToString())
        };

        // Add application claims for regular users
        if (user.UserType == UserType.Regular && applicationId.HasValue)
        {
            var userApp = user.UserApplications
                .FirstOrDefault(ua => ua.ApplicationId == applicationId.Value);

            if (userApp != null)
            {
                claims.Add(new("application_id", userApp.ApplicationId.ToString()));
                claims.Add(new("application_code", userApp.Application.Code));
                claims.Add(new("application_role", userApp.ApplicationRole.Name));
            }
        }

        var accessToken = GenerateJwtToken(claims, TimeSpan.FromMinutes(15));
        var refreshToken = GenerateRefreshToken();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
```

### 4. API Layer (`GenericAuth.API`)

**Purpose**: HTTP endpoints, middleware, configuration.

**Contents:**
```
API/
├── Controllers/
│   └── V1/
│       ├── AuthController.cs
│       ├── UsersController.cs
│       ├── ApplicationsController.cs
│       ├── ApplicationRolesController.cs
│       ├── RolesController.cs
│       └── UserApplicationsController.cs
├── Middleware/
│   └── ApplicationAuthenticationMiddleware.cs
├── Program.cs                 # Application startup
└── appsettings.json          # Configuration
```

**Key Principles:**
- Controllers are thin (delegate to MediatR)
- Middleware handles cross-cutting concerns
- Configuration through dependency injection
- API versioning via URL segments

**Example Controller:**
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "AuthAdminOnly")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));

        if (!result.IsSuccess)
            return NotFound(new { errors = result.Errors });

        return Ok(new { success = true, data = result.Value });
    }
}
```

---

## Multi-Tenant Architecture

GenericAuth implements multi-tenancy through **application-scoped roles** rather than traditional schema/database separation.

### Multi-Tenancy Model

```
┌─────────────────────────────────────────────────────────────┐
│                        GenericAuth                           │
│                     (Single Database)                        │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  Application 1  │  │  Application 2  │  │  Application 3  │
│    (Tenant 1)   │  │    (Tenant 2)   │  │    (Tenant 3)   │
└─────────────────┘  └─────────────────┘  └─────────────────┘
│   App Roles:    │  │   App Roles:    │  │   App Roles:    │
│   - Admin       │  │   - Owner       │  │   - Manager     │
│   - User        │  │   - Member      │  │   - Employee    │
│   - Guest       │  │   - Viewer      │  │   - Customer    │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

### Tenant Isolation Mechanisms

**1. Application Entity**
```csharp
public class Application : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }  // Unique identifier
    public string ApiKeyHash { get; private set; }
    public bool IsActive { get; private set; }

    // Each application has its own roles
    public ICollection<ApplicationRole> ApplicationRoles { get; private set; }
    public ICollection<UserApplication> UserApplications { get; private set; }
}
```

**2. Application Roles (Tenant-Scoped)**
```csharp
public class ApplicationRole : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; }

    public string Name { get; private set; }     // Unique within application
    public string Description { get; private set; }
    public bool IsDefault { get; private set; }  // Default for new users
    public bool IsActive { get; private set; }

    // Permissions specific to this application role
    public ICollection<ApplicationRolePermission> Permissions { get; private set; }
}
```

**3. User-Application Assignment**
```csharp
public class UserApplication : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; }

    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; }

    public Guid ApplicationRoleId { get; private set; }
    public ApplicationRole ApplicationRole { get; private set; }

    public DateTime AssignedAt { get; private set; }
}
```

### JWT Token Claims for Tenancy

**Regular User Token (with Application Context):**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "user_type": "Regular",
  "application_id": "app-guid",           // Tenant identifier
  "application_code": "TENANT_APP",       // Tenant code
  "application_role": "Admin",            // Tenant-specific role
  "permissions": [                         // Tenant-specific permissions
    "users.read",
    "users.write",
    "products.manage"
  ]
}
```

### Tenant Isolation Flow

1. **User Login**:
   - Regular user MUST provide `applicationId`
   - System validates user has access to application
   - Token includes application context

2. **API Request**:
   - Token contains `application_id` claim
   - Middleware can extract tenant context
   - Business logic enforces tenant boundaries

3. **Data Access**:
   - Queries filtered by `ApplicationId`
   - Users can only access their assigned applications
   - Roles and permissions scoped to application

### Advantages of This Approach

**1. Cost-Effective**
- Single database for all tenants
- Reduced infrastructure costs
- Simplified maintenance

**2. Flexibility**
- Users can belong to multiple applications
- Different roles in different applications
- Easy to add new applications

**3. Performance**
- No cross-database queries
- Efficient indexing
- Shared resources

**4. Security**
- Application-level isolation
- JWT claims enforce boundaries
- Cannot access other applications' data

---

## Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)

**Separation of Read and Write Operations**

```csharp
// COMMAND (Write) - Changes state
public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<UserDto>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create user, validate, save to database
        // Business logic here
    }
}

// QUERY (Read) - Returns data
public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Fetch user from database
        // Map to DTO
        // Return result
    }
}
```

**Benefits:**
- Clear separation of concerns
- Optimized read models
- Independent scaling
- Better testability

### 2. Repository Pattern

**Abstraction over data access**

```csharp
// Interface (in Application layer)
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

// Implementation (in Infrastructure layer)
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    // ... other implementations
}
```

**Benefits:**
- Testability (mock repositories)
- Centralized data access logic
- Database independence
- Easier to change ORM

### 3. Unit of Work Pattern

**Manages transactions across repositories**

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Usage in command handler
public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Application> _applicationRepo;
    private readonly IRepository<ApplicationRole> _roleRepo;

    public async Task<Result> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Create application
            var app = new Application(...);
            await _applicationRepo.AddAsync(app);

            // Create initial roles
            foreach (var roleDto in request.InitialRoles)
            {
                var role = new ApplicationRole(...);
                await _roleRepo.AddAsync(role);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### 4. Result Pattern

**Explicit error handling without exceptions**

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public IEnumerable<string> Errors { get; }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public static Result<T> Failure(params string[] errors) => new(false, default, errors);
}

// Usage
public async Task<Result<User>> CreateUser(CreateUserCommand command)
{
    if (await EmailExists(command.Email))
        return Result<User>.Failure("Email already exists.");

    var user = new User(command.Email, command.Password);
    await _repository.AddAsync(user);

    return Result<User>.Success(user);
}

// Caller
var result = await CreateUser(command);
if (!result.IsSuccess)
{
    return BadRequest(new { errors = result.Errors });
}
return Ok(result.Value);
```

**Benefits:**
- Explicit error handling
- Type-safe errors
- Railway-oriented programming
- Better than exception-driven flow

### 5. Specification Pattern

**Encapsulate query logic**

```csharp
public class ActiveUsersSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.IsActive;
    }
}

public class UsersByApplicationSpecification : Specification<User>
{
    private readonly Guid _applicationId;

    public UsersByApplicationSpecification(Guid applicationId)
    {
        _applicationId = applicationId;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.UserApplications
            .Any(ua => ua.ApplicationId == _applicationId);
    }
}

// Usage
var activeUsers = await _repository.FindAsync(new ActiveUsersSpecification());
var appUsers = await _repository.FindAsync(
    new ActiveUsersSpecification()
        .And(new UsersByApplicationSpecification(appId)));
```

### 6. Strategy Pattern

**Different password hashing strategies**

```csharp
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        // PBKDF2 implementation
    }

    public bool Verify(string password, string hash)
    {
        // Verification logic
    }
}

public class BcryptPasswordHasher : IPasswordHasher
{
    // BCrypt implementation
}

// Dependency injection
services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
```

### 7. Mediator Pattern (via MediatR)

**Decouples request/response**

```csharp
// Send command through mediator
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Errors);
    }
}
```

**Benefits:**
- Loose coupling
- Single responsibility
- Pipeline behaviors (validation, logging)
- Easy to test

---

## Domain-Driven Design

### Aggregates

**User Aggregate**
```
User (Aggregate Root)
├── Email (Value Object)
├── PasswordHash (Value Object)
├── UserApplications (Collection)
├── UserRoles (Collection)
└── RefreshTokens (Collection)
```

**Application Aggregate**
```
Application (Aggregate Root)
├── Code (Unique Identifier)
├── ApiKeyHash (Value Object)
├── ApplicationRoles (Collection)
└── UserApplications (Collection)
```

### Value Objects

**Email Value Object**
```csharp
public class Email : ValueObject
{
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format.");

        return new Email(email.ToLowerInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Benefits:**
- Immutability
- Encapsulation of validation
- Value-based equality
- Self-documenting code

### Domain Events

```csharp
public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public UserType UserType { get; }

    public UserCreatedEvent(Guid userId, string email, UserType userType)
    {
        UserId = userId;
        Email = email;
        UserType = userType;
    }
}

// Raise event in aggregate
public class User : BaseEntity
{
    public static User Create(string firstName, string lastName, Email email, PasswordHash passwordHash)
    {
        var user = new User
        {
            // ... set properties
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, email.Value, user.UserType));

        return user;
    }
}
```

### Bounded Contexts

```
┌─────────────────────────────────────────────────────────────┐
│                    Identity Context                          │
│  - User Authentication                                       │
│  - Password Management                                       │
│  - Token Generation                                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                 Authorization Context                        │
│  - Roles and Permissions                                     │
│  - User-Application Assignments                              │
│  - Access Control                                            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                Multi-Tenancy Context                         │
│  - Application Management                                    │
│  - Application Roles                                         │
│  - Tenant Isolation                                          │
└─────────────────────────────────────────────────────────────┘
```

---

## CQRS Implementation

### Command Flow

```
HTTP POST Request
        │
        ▼
   Controller
        │
        ▼
   Command DTO
        │
        ▼
  MediatR Pipeline
        │
        ├─► Validation Behavior (FluentValidation)
        │
        ├─► Logging Behavior
        │
        ▼
 Command Handler
        │
        ├─► Load Domain Entities
        │
        ├─► Execute Business Logic
        │
        ├─► Save Changes (Unit of Work)
        │
        ▼
   Result<T>
        │
        ▼
    Controller
        │
        ▼
  HTTP Response
```

### Query Flow

```
HTTP GET Request
        │
        ▼
   Controller
        │
        ▼
   Query DTO
        │
        ▼
  MediatR Pipeline
        │
        ▼
  Query Handler
        │
        ├─► Execute Database Query (EF Core)
        │
        ├─► Map to DTO (AutoMapper)
        │
        ▼
   Result<T>
        │
        ▼
    Controller
        │
        ▼
  HTTP Response (JSON)
```

### Pipeline Behaviors

**Validation Behavior**
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

---

## Security Architecture

### Authentication Flow

```
1. User submits credentials
        │
        ▼
2. Verify password hash (PBKDF2)
        │
        ▼
3. Validate user is active
        │
        ▼
4. Check application assignment (Regular users)
        │
        ▼
5. Generate JWT access token (15 min)
        │
        ▼
6. Generate refresh token (7 days)
        │
        ▼
7. Store refresh token in database
        │
        ▼
8. Return tokens to client
```

### Password Security

**PBKDF2 with Salt**
```csharp
public class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000;

    public string Hash(string password)
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
```

### JWT Token Security

**Token Configuration**
- Algorithm: HS256 (HMAC-SHA256)
- Access Token Expiry: 15 minutes
- Refresh Token Expiry: 7 days
- Claims: User ID, Email, User Type, Application Context

**Token Validation**
- Issuer validation
- Audience validation
- Lifetime validation
- Signature validation
- No clock skew tolerance

### Authorization Policies

```csharp
services.AddAuthorization(options =>
{
    // Auth Admin only
    options.AddPolicy("AuthAdminOnly", policy =>
        policy.RequireClaim("user_type", "AuthAdmin"));

    // Require application context
    options.AddPolicy("RequireApplication", policy =>
        policy.RequireClaim("application_id"));

    // Application admin role
    options.AddPolicy("RequireApplicationAdmin", policy =>
        policy.RequireClaim("application_role", "Admin"));
});
```

### Refresh Token Rotation

```
1. Client sends refresh token
        │
        ▼
2. Validate token exists and not expired
        │
        ▼
3. Revoke old refresh token
        │
        ▼
4. Generate new access token
        │
        ▼
5. Generate new refresh token
        │
        ▼
6. Store new refresh token
        │
        ▼
7. Return new tokens
```

**Benefits:**
- Limits token lifetime
- Detects token theft
- Provides logout capability
- Reduces risk of token reuse

---

## API Versioning

### Strategy: URL Path Versioning

```
/api/v1/users
/api/v2/users
```

**Configuration:**
```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

**Controller Implementation:**
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    // Endpoints for version 1.0
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersV2Controller : ControllerBase
{
    // Endpoints for version 2.0
}
```

### Version Deprecation Strategy

1. Announce deprecation in response headers
2. Provide migration guide
3. Support old version for 6 months
4. Return deprecation warnings
5. Remove old version after sunset period

---

## Data Flow

### Complete Request Flow Example

**User Login Request**

```
1. HTTP POST /api/v1/auth/login
   ├─ Body: { email, password, applicationId }
   └─ Headers: Content-Type: application/json

2. API Layer (AuthController)
   ├─ Receive LoginRequest
   ├─ Map to LoginCommand
   └─ Send to MediatR

3. Application Layer (MediatR Pipeline)
   ├─ Validation Behavior
   │  ├─ LoginCommandValidator
   │  ├─ Validate email format
   │  └─ Validate required fields
   ├─ Logging Behavior
   │  └─ Log request details
   └─ LoginCommandHandler
       ├─ Query database for user
       ├─ Verify password hash
       ├─ Check user is active
       ├─ Validate application assignment
       ├─ Generate JWT tokens
       ├─ Store refresh token
       └─ Return Result<LoginResponse>

4. Infrastructure Layer
   ├─ ApplicationDbContext
   │  └─ EF Core query to Users table
   ├─ IdentityService
   │  └─ PBKDF2 password verification
   └─ TokenService
       ├─ Generate access token (JWT)
       └─ Generate refresh token

5. API Layer (Response)
   ├─ Check Result.IsSuccess
   ├─ Map to response format
   └─ Return HTTP 200 OK with tokens

6. HTTP Response
   └─ JSON: { success: true, data: { accessToken, refreshToken } }
```

---

## Technology Stack

### Backend Framework
- **.NET 8.0**: Latest LTS version
- **ASP.NET Core**: Web API framework
- **C# 12**: Modern language features

### Architecture & Patterns
- **Clean Architecture**: Layer separation
- **DDD**: Domain-driven design
- **CQRS**: Command/Query separation
- **MediatR**: Mediator pattern

### Data Access
- **Entity Framework Core 8**: ORM
- **SQL Server**: Relational database
- **LINQ**: Query language

### Validation & Mapping
- **FluentValidation**: Input validation
- **AutoMapper**: Object mapping

### Authentication & Security
- **JWT Bearer**: Token authentication
- **PBKDF2**: Password hashing
- **ASP.NET Core Identity**: (Infrastructure only, not full Identity system)

### Testing
- **xUnit**: Testing framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking library
- **WebApplicationFactory**: Integration testing

### Documentation
- **Swagger/OpenAPI**: API documentation
- **Swashbuckle**: Swagger generator

### DevOps
- **GitHub Actions**: CI/CD
- **Docker**: Containerization (future)
- **Kubernetes**: Orchestration (future)

---

## Best Practices

### 1. Command/Query Naming
```
Commands: VerbNoun (CreateUser, UpdateRole, DeleteApplication)
Queries: GetNounByIdentifier (GetUserById, GetApplicationByCode)
```

### 2. Result Pattern Usage
Always return `Result<T>` from handlers:
```csharp
// Good
public async Task<Result<UserDto>> Handle(GetUserByIdQuery request)
{
    var user = await _repository.GetByIdAsync(request.Id);
    if (user == null)
        return Result<UserDto>.Failure("User not found.");

    return Result<UserDto>.Success(MapToDto(user));
}

// Bad - throws exception
public async Task<UserDto> Handle(GetUserByIdQuery request)
{
    var user = await _repository.GetByIdAsync(request.Id);
    if (user == null)
        throw new NotFoundException("User not found.");

    return MapToDto(user);
}
```

### 3. Domain Entity Encapsulation
```csharp
// Good - Private setters, public methods
public class User : BaseEntity
{
    public string FirstName { get; private set; }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        FirstName = firstName;
        LastName = lastName;
    }
}

// Bad - Public setters
public class User : BaseEntity
{
    public string FirstName { get; set; }
}
```

### 4. Async/Await Best Practices
```csharp
// Good - ConfigureAwait(false) in libraries
public async Task<User> GetUserAsync(Guid id)
{
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);
}

// Good - Async all the way
public async Task<IActionResult> GetUser(Guid id)
{
    var result = await _mediator.Send(new GetUserByIdQuery(id));
    return Ok(result);
}
```

---

## Scalability Considerations

### Horizontal Scaling
- Stateless API (no in-memory sessions)
- JWT tokens (no server-side session storage)
- Database connection pooling
- Read replicas for queries

### Caching Strategy
```csharp
// Cache frequently accessed data
services.AddMemoryCache();
services.AddDistributedRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// Use in queries
public async Task<Result<AppDto>> Handle(GetApplicationByCodeQuery request)
{
    var cacheKey = $"app:{request.Code}";

    if (_cache.TryGetValue(cacheKey, out AppDto cachedApp))
        return Result<AppDto>.Success(cachedApp);

    var app = await _context.Applications
        .FirstOrDefaultAsync(a => a.Code == request.Code);

    _cache.Set(cacheKey, app, TimeSpan.FromMinutes(15));

    return Result<AppDto>.Success(app);
}
```

### Performance Optimization
- Use projections in queries (Select only needed columns)
- Implement pagination for large datasets
- Use AsNoTracking() for read-only queries
- Index frequently queried columns
- Use compiled queries for hot paths

---

**Last Updated:** November 9, 2025
**Version:** 1.0
