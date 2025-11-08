# GenericAuth - Architecture Design Document

## Executive Summary
This document outlines the architectural design for GenericAuth, a .NET Core API implementing Clean Architecture, Domain-Driven Design (DDD), CQRS pattern with MediatR, and microservice-ready patterns.

## 1. High-Level Architecture

### Architecture Pattern: Clean Architecture + DDD + CQRS

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  (Controllers, Middleware, Filters, API Gateway Ready)       │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                    Application Layer                         │
│  (CQRS: Commands/Queries, Handlers, DTOs, MediatR)          │
│  (Application Services, Validators, Mappers)                 │
└─────────┬─────────────────────────────────┬─────────────────┘
          │                                 │
┌─────────▼─────────┐              ┌────────▼─────────────────┐
│   Domain Layer    │              │  Infrastructure Layer     │
│  (DDD Patterns)   │              │  (EF Core, Dapper, Auth)  │
│                   │              │                           │
│ - Entities        │              │ - DbContext (EF Core)     │
│ - Value Objects   │              │ - Dapper Queries          │
│ - Aggregates      │              │ - Repositories            │
│ - Domain Events   │              │ - Identity/JWT            │
│ - Domain Services │              │ - External Services       │
│ - Interfaces      │              │ - Event Handlers          │
└───────────────────┘              └───────────────────────────┘
```

## 2. Solution Structure

```
GenericAuth/
├── src/
│   ├── GenericAuth.Domain/                    # Core Domain Layer
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   └── Permission.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   ├── Password.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Aggregates/
│   │   │   └── UserAggregate/
│   │   ├── Events/
│   │   │   ├── UserCreatedEvent.cs
│   │   │   ├── UserLoggedInEvent.cs
│   │   │   └── PasswordChangedEvent.cs
│   │   ├── Exceptions/
│   │   │   └── DomainException.cs
│   │   ├── Interfaces/
│   │   │   └── IRepository.cs
│   │   └── Services/
│   │       └── IPasswordHasher.cs
│   │
│   ├── GenericAuth.Application/               # Application Layer (CQRS)
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── TransactionBehavior.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   └── IQueryDbConnection.cs
│   │   │   ├── Models/
│   │   │   │   └── Result.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs
│   │   ├── Features/
│   │   │   ├── Authentication/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── Login/
│   │   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   │   │   └── LoginCommandValidator.cs
│   │   │   │   │   ├── Register/
│   │   │   │   │   │   ├── RegisterCommand.cs
│   │   │   │   │   │   ├── RegisterCommandHandler.cs
│   │   │   │   │   │   └── RegisterCommandValidator.cs
│   │   │   │   │   └── RefreshToken/
│   │   │   │   └── Queries/
│   │   │   │       └── GetUserProfile/
│   │   │   │           ├── GetUserProfileQuery.cs
│   │   │   │           └── GetUserProfileQueryHandler.cs
│   │   │   └── Users/
│   │   │       ├── Commands/
│   │   │       │   ├── CreateUser/
│   │   │       │   ├── UpdateUser/
│   │   │       │   └── DeleteUser/
│   │   │       └── Queries/
│   │   │           ├── GetUsers/
│   │   │           └── GetUserById/
│   │   └── DependencyInjection.cs
│   │
│   ├── GenericAuth.Infrastructure/            # Infrastructure Layer
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   └── RoleConfiguration.cs
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── DapperDbConnection.cs
│   │   │   └── Repositories/
│   │   │       └── Repository.cs
│   │   ├── Identity/
│   │   │   ├── JwtTokenGenerator.cs
│   │   │   ├── PasswordHasher.cs
│   │   │   └── IdentityService.cs
│   │   ├── Services/
│   │   │   ├── DateTimeService.cs
│   │   │   └── EmailService.cs
│   │   ├── Migrations/
│   │   └── DependencyInjection.cs
│   │
│   └── GenericAuth.API/                       # Presentation Layer
│       ├── Controllers/
│       │   ├── AuthenticationController.cs
│       │   └── UsersController.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── Filters/
│       │   └── ApiExceptionFilterAttribute.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
├── tests/
│   ├── GenericAuth.Domain.UnitTests/
│   ├── GenericAuth.Application.UnitTests/
│   ├── GenericAuth.Infrastructure.IntegrationTests/
│   └── GenericAuth.API.IntegrationTests/
│
└── GenericAuth.sln
```

## 3. Bounded Contexts (DDD)

### Primary Bounded Context: Identity & Access Management

**Aggregates:**

1. **User Aggregate** (Aggregate Root: User)
   - User Entity (Root)
   - Role Entities
   - Permission Value Objects
   - UserProfile Value Object
   - RefreshToken Value Object

2. **Role Aggregate** (Aggregate Root: Role)
   - Role Entity (Root)
   - Permission Value Objects
   - Role Claims

**Domain Events:**
- UserRegisteredEvent
- UserLoggedInEvent
- UserLoggedOutEvent
- PasswordChangedEvent
- RoleAssignedEvent
- PermissionGrantedEvent

## 4. CQRS Implementation Strategy

### Command Side (Write Model)
- **Technology**: EF Core Code First
- **Purpose**: Handle all state-changing operations
- **Features**:
  - Transaction support
  - Domain event publishing
  - Optimistic concurrency
  - Audit trails

### Query Side (Read Model)
- **Technology**: Dapper
- **Purpose**: High-performance read operations
- **Features**:
  - Direct SQL queries
  - No change tracking overhead
  - DTO projections
  - Optimized for reads

### MediatR Integration
```csharp
// Commands
IRequest<Result> → CommandHandler → EF Core → Database

// Queries
IRequest<TResponse> → QueryHandler → Dapper → Database
```

## 5. Microservice Readiness Patterns

### 1. API Gateway Pattern
- Centralized entry point
- Request routing
- Authentication/Authorization
- Rate limiting

### 2. Service Communication
- **Synchronous**: HTTP/REST, gRPC
- **Asynchronous**: Message Queue (RabbitMQ, Azure Service Bus)
- **Domain Events**: MediatR (in-process) → Message Bus (cross-service)

### 3. Data Management
- **Database per Service**: Each microservice owns its data
- **Saga Pattern**: For distributed transactions
- **Outbox Pattern**: For reliable event publishing

### 4. Resilience Patterns
- Circuit Breaker (Polly)
- Retry with exponential backoff
- Timeout policies
- Bulkhead isolation

### 5. Service Discovery
- Configuration-based (appsettings)
- Consul/Eureka integration ready
- Docker/Kubernetes service mesh ready

## 6. Authentication & Authorization Architecture

### Recommended Approach: JWT + Claims-Based

**Technology Stack:**
- ASP.NET Core Identity (optional, for user management)
- JWT Bearer Authentication
- Claims-based Authorization
- Refresh Token mechanism

**Implementation:**

1. **Authentication Flow:**
   ```
   User → Login → Validate Credentials → Generate JWT + Refresh Token → Return Tokens
   ```

2. **Authorization:**
   - Policy-based authorization
   - Claims-based authorization
   - Role-based authorization
   - Resource-based authorization

3. **Clean Architecture Placement:**
   - **Domain Layer**: User, Role, Permission entities
   - **Application Layer**: Login/Register commands, JWT interfaces
   - **Infrastructure Layer**: JWT generation, password hashing
   - **API Layer**: [Authorize] attributes, auth middleware

### Security Features:
- Password hashing (BCrypt/PBKDF2)
- Refresh token rotation
- Token revocation
- Rate limiting
- CORS configuration
- HTTPS enforcement

## 7. Technology Stack

### Core Frameworks
- **.NET Version**: .NET 8.0 (latest LTS)
- **Language**: C# 12

### NuGet Packages

#### Domain Layer
- No external dependencies (pure domain logic)

#### Application Layer
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
```

#### Infrastructure Layer
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Dapper" Version="2.1.28" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
```

#### API Layer
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

#### Testing
```xml
<PackageReference Include="xUnit" Version="2.6.4" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

## 8. Key Design Decisions & Trade-offs

### Decision 1: Vertical Slice vs Horizontal Layers
**Chosen**: Hybrid Approach
- **Horizontal layers** for Domain and Infrastructure (cross-cutting)
- **Vertical slices** for Application layer (Features folder)
- **Rationale**: Combines discoverability with feature cohesion

### Decision 2: EF Core + Dapper
**Chosen**: Use both
- EF Core for Commands (writes)
- Dapper for Queries (reads)
- **Rationale**: Optimizes for CQRS pattern, performance on reads

### Decision 3: Aggregates Design
**Chosen**: Small, focused aggregates
- User Aggregate: User + immediate value objects
- Separate Role Aggregate
- **Rationale**: Better concurrency, clearer boundaries

### Decision 4: Domain Events
**Chosen**: MediatR notifications (in-process)
- Will extend to message bus for microservices
- **Rationale**: Start simple, scale to distributed

### Decision 5: Authentication
**Chosen**: JWT with Refresh Tokens
- Stateless authentication
- Scalable across microservices
- **Rationale**: Industry standard, microservice-friendly

## 9. Enterprise Patterns Included

1. **Repository Pattern**: Abstraction over data access
2. **Unit of Work**: Transaction management
3. **Specification Pattern**: Reusable query logic
4. **Result Pattern**: Functional error handling
5. **Pipeline Behavior**: Cross-cutting concerns (validation, logging)
6. **Factory Pattern**: Complex object creation
7. **Strategy Pattern**: Authentication strategies
8. **Mediator Pattern**: Decoupled request handling

## 10. Best Practices & Conventions

### Naming Conventions
- **Commands**: `{Verb}{Noun}Command` (e.g., CreateUserCommand)
- **Queries**: `Get{Noun}Query` (e.g., GetUserByIdQuery)
- **Handlers**: `{Command/Query}Handler`
- **Validators**: `{Command/Query}Validator`

### Code Organization
- Feature folders in Application layer
- One feature = one folder with Commands/Queries
- DTOs alongside their queries

### Dependency Flow
```
API → Application → Domain ← Infrastructure
```
- Domain has NO dependencies
- Application depends on Domain
- Infrastructure depends on Domain and Application
- API depends on all layers (composition root)

## 11. Next Steps for Implementation

1. Create solution structure
2. Set up Domain layer with core entities
3. Configure Application layer with MediatR
4. Set up Infrastructure with EF Core + Dapper
5. Create API project with JWT auth
6. Implement example CQRS flow
7. Add unit and integration tests
8. Configure Docker for containerization
9. Add API documentation (Swagger)
10. Set up CI/CD pipeline

## 12. Future Microservice Split Strategy

When ready to split into microservices:

**Potential Services:**
1. **AuthService**: Authentication, JWT generation
2. **UserService**: User management, profiles
3. **RoleService**: Role and permission management
4. **NotificationService**: Email, SMS notifications

**Shared Infrastructure:**
- API Gateway (Ocelot/YARP)
- Message Bus (RabbitMQ/Azure Service Bus)
- Distributed tracing (OpenTelemetry)
- Centralized logging (ELK/Seq)
- Service mesh (Istio/Linkerd) for Kubernetes
