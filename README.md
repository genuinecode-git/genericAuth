# GenericAuth - Clean Architecture .NET Core API

[![CI/CD Pipeline](https://github.com/genuinecode-git/genericAuth/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/genuinecode-git/genericAuth/actions/workflows/ci-cd.yml)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Coverage](https://img.shields.io/badge/coverage-%E2%89%A580%25-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

A production-ready multi-tenant authentication and authorization API built with Clean Architecture, Domain-Driven Design (DDD), CQRS pattern with MediatR, and comprehensive role-based access control.

## Architecture Overview

This project follows Clean Architecture principles with clear separation of concerns across four layers:

- **Domain Layer**: Core business logic, entities, value objects, domain events
- **Application Layer**: Use cases, CQRS (commands & queries), MediatR handlers
- **Infrastructure Layer**: Data access (EF Core for writes, Dapper for reads), external services
- **API Layer**: HTTP endpoints, authentication, middleware

## Technology Stack

- **.NET 8.0** with C# 12
- **EF Core 8.0** for data access
- **MediatR** for CQRS implementation
- **FluentValidation** for request validation
- **JWT Bearer** authentication with refresh tokens
- **SQL Server** database
- **xUnit** for unit and integration testing
- **Swagger/OpenAPI** for API documentation

## What is GenericAuth?

GenericAuth is a comprehensive multi-tenant authentication and authorization system that provides:

- **Multi-Tenant Architecture**: Application-scoped roles for complete tenant isolation
- **Dual User Types**: Auth Admins (system administrators) and Regular Users (application users)
- **JWT Authentication**: Secure token-based authentication with automatic refresh
- **Role-Based Access Control**: Granular permissions for both system and application roles
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Command Query Responsibility Segregation using MediatR
- **Production-Ready**: Comprehensive testing, CI/CD, and enterprise-grade code quality

## Quick Start

```bash
# Build the solution
dotnet build

# Run the API (migrations and seeding happen automatically on startup)
cd src/GenericAuth.API
dotnet run
```

### Default Credentials

The system automatically seeds a default **Auth Admin** user on first run:

| Field | Value |
|-------|-------|
| **Email** | `admin@genericauth.com` |
| **Password** | `Admin@123` |
| **User Type** | `AuthAdmin` (System Administrator) |

⚠️ **SECURITY WARNING**: Change the default password immediately in production environments!

### Testing the API

1. Navigate to Swagger UI at: `https://localhost:{port}` (port will be shown in console)
2. Use the default Auth Admin credentials to login
3. Follow the [Testing Guide](./TESTING_GUIDE.md) for complete end-to-end testing workflows

## Git Workflow & Branching Strategy

This project follows a **Git Flow** workflow with protected branches:

### Branches
- **`main`** - Production-ready code (protected, requires PR)
- **`development`** - Active development (default branch)
- **`feature/*`** - New features (merge to development)
- **`bugfix/*`** - Bug fixes (merge to development)
- **`hotfix/*`** - Critical fixes (merge to main and development)

### Development Workflow

```bash
# Start new feature
git checkout development
git pull origin development
git checkout -b feature/your-feature-name

# Work on feature, commit changes
git add .
git commit -m "feat: Add new feature"
git push origin feature/your-feature-name

# Create Pull Request on GitHub
# After approval and CI/CD passes, merge to development
```

**Important**: All changes to `main` and `development` must go through Pull Requests. Direct commits are not allowed.

See [Git Workflow Documentation](./.github/GIT_WORKFLOW.md) for detailed guidelines.

## CI/CD Pipeline

This project includes a comprehensive CI/CD pipeline that runs on every push to `main` and on pull requests to `main`:

- ✅ **Automated Builds**: Compiles the entire solution
- ✅ **Test Execution**: Runs all unit and integration tests
- ✅ **Code Coverage**: Enforces minimum 80% code coverage
- ✅ **Quality Gates**: Fails if tests fail or coverage is below threshold
- ✅ **Artifacts**: Uploads test results and coverage reports
- ✅ **PR Checks**: All pull requests are validated before merge

See [CI/CD Documentation](./.github/CICD_DOCUMENTATION.md) for detailed information.

### Running Tests Locally

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# View coverage report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:"Html"
open ./coverage/report/index.html
```

## Documentation

### API Documentation
- **[API Reference](./docs/API_REFERENCE.md)** - Complete API endpoint documentation with examples
- **[Getting Started Guide](./docs/GETTING_STARTED.md)** - Quick start guide and common workflows
- **[Architecture Guide](./docs/ARCHITECTURE_GUIDE.md)** - System architecture, patterns, and design
- **[Postman Collection](./docs/GenericAuth.postman_collection.json)** - Import into Postman for testing

### Development
- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Original architectural decisions and design rationale
- **[Testing Guide](./TESTING_GUIDE.md)** - Complete testing workflows and scenarios
- **[Git Workflow](./.github/GIT_WORKFLOW.md)** - Branching strategy and development workflow
- **[CI/CD Documentation](./.github/CICD_DOCUMENTATION.md)** - GitHub Actions pipeline and coverage
- **[Pull Request Template](./.github/pull_request_template.md)** - PR template and guidelines

## Key Features

### Multi-Tenant Authentication
- **Application-Scoped Roles**: Each application (tenant) has isolated roles
- **Dual User Types**:
  - **Auth Admins**: System administrators managing all applications
  - **Regular Users**: Application users with tenant-specific access
- **JWT Token Authentication**: Secure token-based auth with 15-minute access tokens
- **Refresh Token Rotation**: Automatic token refresh with 7-day refresh tokens
- **Password Security**: PBKDF2 hashing with 100,000 iterations
- **Multi-Application Access**: Users can belong to multiple applications with different roles

### API Features
- **Complete CRUD Operations**: Users, Applications, Roles, Permissions
- **User Management**: Registration, activation, deactivation, profile updates
- **Application Management**: Create applications with API keys and initial roles
- **Role Management**: Both system roles (Auth Admin) and application roles (Regular users)
- **Permission Management**: Granular permission assignment to roles
- **User-Application Assignments**: Assign users to applications with specific roles
- **Password Reset Flow**: Forgot password and reset password with tokens

### Architecture & Patterns
- **Clean Architecture**: Clear separation of Domain, Application, Infrastructure, and API layers
- **Domain-Driven Design**: Rich domain models, aggregates, value objects, domain events
- **CQRS Pattern**: Command Query Responsibility Segregation using MediatR
- **Repository Pattern**: Abstraction over data access
- **Unit of Work**: Transaction management
- **Result Pattern**: Explicit error handling without exceptions
- **Specification Pattern**: Encapsulated query logic
- **Strategy Pattern**: Pluggable password hashing strategies

### Security
- **JWT Bearer Authentication**: Industry-standard token authentication
- **Authorization Policies**: Policy-based authorization (AuthAdminOnly, RequireApplication)
- **Password Requirements**: Enforced complexity rules
- **API Key Authentication**: SHA-256 hashed API keys for applications
- **Token Validation**: Issuer, audience, lifetime, and signature validation
- **Refresh Token Security**: Single-use tokens with automatic rotation
- **CORS Configuration**: Configurable cross-origin resource sharing

### Development Experience
- **Swagger/OpenAPI**: Interactive API documentation at root URL
- **Automatic Seeding**: Default Auth Admin user on first run
- **Comprehensive Validation**: FluentValidation for all inputs
- **Detailed Error Messages**: User-friendly validation and error responses
- **Postman Collection**: Ready-to-use API testing collection
- **Integration Tests**: 80%+ code coverage with comprehensive test suite

## Project Structure

```
genericAuth/
├── src/
│   ├── GenericAuth.API/              # API Layer (Controllers, Middleware)
│   │   ├── Controllers/V1/           # Versioned API controllers
│   │   ├── Middleware/               # Application authentication middleware
│   │   └── Program.cs                # Application startup and configuration
│   ├── GenericAuth.Application/      # Application Layer (CQRS, MediatR)
│   │   ├── Features/                 # Organized by feature (not by layer)
│   │   │   ├── Authentication/       # Login, Register, Refresh, etc.
│   │   │   ├── Users/                # User management commands/queries
│   │   │   ├── Applications/         # Application management
│   │   │   ├── ApplicationRoles/     # Application role management
│   │   │   ├── Roles/                # System role management
│   │   │   └── UserApplications/     # User-Application assignments
│   │   ├── Common/                   # Shared interfaces, models, behaviors
│   │   └── DependencyInjection.cs    # Service registration
│   ├── GenericAuth.Domain/           # Domain Layer (Entities, Value Objects)
│   │   ├── Entities/                 # Domain entities (User, Application, etc.)
│   │   ├── ValueObjects/             # Value objects (Email, PasswordHash)
│   │   ├── Enums/                    # Domain enumerations
│   │   ├── Events/                   # Domain events
│   │   └── Common/                   # Base entity classes
│   └── GenericAuth.Infrastructure/   # Infrastructure Layer (Data Access, Services)
│       ├── Persistence/              # EF Core DbContext, configurations
│       │   ├── Configurations/       # Entity type configurations
│       │   ├── Migrations/           # Database migrations
│       │   └── DatabaseSeeder.cs     # Data seeding
│       ├── Identity/                 # Password hashing, JWT services
│       └── DependencyInjection.cs    # Infrastructure service registration
├── tests/
│   ├── GenericAuth.Application.UnitTests/        # Unit tests for Application layer
│   └── GenericAuth.API.IntegrationTests/         # Integration tests for API
├── docs/
│   ├── API_REFERENCE.md              # Complete API documentation
│   ├── GETTING_STARTED.md            # Quick start guide
│   ├── ARCHITECTURE_GUIDE.md         # Architecture deep dive
│   └── GenericAuth.postman_collection.json  # Postman collection
└── .github/
    └── workflows/
        └── ci-cd.yml                 # CI/CD pipeline
```

## API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - User login (Auth Admin or Regular)
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - Logout and revoke tokens
- `POST /api/v1/auth/forgot-password` - Request password reset
- `POST /api/v1/auth/reset-password` - Reset password with token

### Users (Auth Admin Only)
- `GET /api/v1/users` - List users (paginated)
- `GET /api/v1/users/{id}` - Get user by ID
- `PUT /api/v1/users/{id}` - Update user
- `POST /api/v1/users/{id}/activate` - Activate user
- `POST /api/v1/users/{id}/deactivate` - Deactivate user
- `POST /api/v1/users/{userId}/roles/{roleId}` - Assign system role
- `DELETE /api/v1/users/{userId}/roles/{roleId}` - Remove system role

### Applications (Auth Admin Only)
- `POST /api/v1/applications` - Create application with roles
- `GET /api/v1/applications/by-code/{code}` - Get application by code

### Application Roles (Auth Admin Only)
- `GET /api/v1/applications/{appId}/roles` - List application roles
- `GET /api/v1/applications/{appId}/roles/{roleId}` - Get role details
- `POST /api/v1/applications/{appId}/roles` - Create role
- `PUT /api/v1/applications/{appId}/roles/{roleId}` - Update role
- `DELETE /api/v1/applications/{appId}/roles/{roleId}` - Delete role
- `POST /api/v1/applications/{appId}/roles/{roleId}/activate` - Activate role
- `POST /api/v1/applications/{appId}/roles/{roleId}/deactivate` - Deactivate role
- `POST /api/v1/applications/{appId}/roles/{roleId}/set-default` - Set as default
- `POST /api/v1/applications/{appId}/roles/{roleId}/permissions/{permissionId}` - Add permission
- `DELETE /api/v1/applications/{appId}/roles/{roleId}/permissions/{permissionId}` - Remove permission

### System Roles (Auth Admin Only)
- `GET /api/v1/roles` - List system roles
- `GET /api/v1/roles/{id}` - Get system role
- `POST /api/v1/roles` - Create system role
- `PUT /api/v1/roles/{id}` - Update system role
- `DELETE /api/v1/roles/{id}` - Delete system role
- `POST /api/v1/roles/{id}/activate` - Activate role
- `POST /api/v1/roles/{id}/deactivate` - Deactivate role
- `POST /api/v1/roles/{id}/permissions/{permissionId}` - Add permission
- `DELETE /api/v1/roles/{id}/permissions/{permissionId}` - Remove permission

### User-Application Assignments (Auth Admin Only)
- `POST /api/v1/user-applications` - Assign user to application
- `GET /api/v1/user-applications/users/{userId}/applications` - Get user's applications
- `GET /api/v1/user-applications/applications/{appCode}/users` - Get application's users
- `PUT /api/v1/user-applications/users/{userId}/applications/{appCode}/role` - Change user role
- `DELETE /api/v1/user-applications/users/{userId}/applications/{appCode}` - Remove user from app

See **[API Reference](./docs/API_REFERENCE.md)** for complete documentation with examples.

## Contributing

We welcome contributions! Please follow these guidelines:

1. **Fork the repository** and create your feature branch from `development`
2. **Follow the Git Workflow** described in [GIT_WORKFLOW.md](./.github/GIT_WORKFLOW.md)
3. **Write tests** for new features (maintain 80%+ coverage)
4. **Update documentation** if you change APIs or add features
5. **Create a Pull Request** following the [PR template](./.github/pull_request_template.md)
6. **Ensure CI/CD passes** before requesting review

### Code Style
- Follow C# coding conventions
- Use meaningful names for variables and methods
- Write XML documentation comments for public APIs
- Keep methods small and focused (Single Responsibility Principle)
- Use async/await for I/O operations

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For questions, issues, or feature requests:
- **Documentation**: See [docs/](./docs/) folder
- **API Reference**: [API_REFERENCE.md](./docs/API_REFERENCE.md)
- **Getting Started**: [GETTING_STARTED.md](./docs/GETTING_STARTED.md)
- **Architecture**: [ARCHITECTURE_GUIDE.md](./docs/ARCHITECTURE_GUIDE.md)

---

**Built with Clean Architecture, DDD, and CQRS principles for enterprise-grade applications.**
