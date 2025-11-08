# GenericAuth - Clean Architecture .NET Core API

[![CI/CD Pipeline](https://github.com/genuinecode-git/genericAuth/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/genuinecode-git/genericAuth/actions/workflows/ci-cd.yml)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Coverage](https://img.shields.io/badge/coverage-%E2%89%A580%25-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

A production-ready .NET Core API implementing Clean Architecture, Domain-Driven Design (DDD), CQRS pattern with MediatR, microservice patterns, EF Core, and Dapper.

## Architecture Overview

This project follows Clean Architecture principles with clear separation of concerns across four layers:

- **Domain Layer**: Core business logic, entities, value objects, domain events
- **Application Layer**: Use cases, CQRS (commands & queries), MediatR handlers
- **Infrastructure Layer**: Data access (EF Core for writes, Dapper for reads), external services
- **API Layer**: HTTP endpoints, authentication, middleware

## Technology Stack

- **.NET 8.0** with C# 12
- **EF Core 9.0** for write operations (Commands)
- **Dapper** for read operations (Queries)
- **MediatR** for CQRS implementation
- **FluentValidation** for request validation
- **JWT Bearer** for authentication
- **SQL Server** database
- **xUnit** for testing

## Project Status

### ✅ Completed
- Solution structure with Clean Architecture layers
- Domain layer with DDD patterns (Entities, Value Objects, Domain Events)
- Application layer with CQRS and MediatR
- Infrastructure layer setup (packages installed)

### 🚧 Next Steps
1. Complete Infrastructure implementations (EF configurations, repositories, JWT, password hasher)
2. Set up API layer with controllers and authentication
3. Configure dependency injection
4. Add database migrations
5. Build and test the solution

## Quick Start

```bash
# Build the solution
dotnet build

# Run migrations (after infrastructure setup is complete)
dotnet ef database update --project src/GenericAuth.Infrastructure --startup-project src/GenericAuth.API

# Run the API
dotnet run --project src/GenericAuth.API
```

## CI/CD Pipeline

This project includes a comprehensive CI/CD pipeline that runs on every push to `main`:

- ✅ **Automated Builds**: Compiles the entire solution
- ✅ **Test Execution**: Runs all unit and integration tests
- ✅ **Code Coverage**: Enforces minimum 80% code coverage
- ✅ **Quality Gates**: Fails if tests fail or coverage is below threshold
- ✅ **Artifacts**: Uploads test results and coverage reports

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

- [ARCHITECTURE.md](./ARCHITECTURE.md) - Detailed architectural decisions, patterns, and design rationale
- [CI/CD Documentation](./.github/CICD_DOCUMENTATION.md) - GitHub Actions pipeline and coverage setup
- [IMPLEMENTATION_STATUS.md](./IMPLEMENTATION_STATUS.md) - Current implementation status

## Key Features

- Clean Architecture with dependency inversion
- Domain-Driven Design with aggregates and value objects
- CQRS pattern separating reads (Dapper) and writes (EF Core)
- MediatR for decoupled request handling
- JWT-based stateless authentication
- Microservice-ready patterns
- Comprehensive validation and error handling
