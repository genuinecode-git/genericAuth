# GenericAuth Documentation

Welcome to the GenericAuth API documentation. This folder contains comprehensive guides to help you understand, integrate, and extend the GenericAuth multi-tenant authentication system.

## Documentation Overview

### Quick Start
- **[Getting Started Guide](./GETTING_STARTED.md)** - Get up and running in 5 minutes
  - Installation and setup
  - First API calls
  - Common workflows
  - Development setup
  - Troubleshooting

### API Documentation
- **[API Reference](./API_REFERENCE.md)** - Complete API endpoint documentation
  - All 39+ endpoints with examples
  - Request/response formats
  - Authentication and authorization
  - Error handling
  - Data models
  - cURL examples for every endpoint

### Architecture
- **[Architecture Guide](./ARCHITECTURE_GUIDE.md)** - Deep dive into system design
  - Clean Architecture layers
  - Multi-tenant architecture
  - Design patterns (CQRS, Repository, Result, etc.)
  - Domain-Driven Design
  - Security architecture
  - Data flow diagrams
  - Technology stack

### API Testing
- **[Postman Collection](./GenericAuth.postman_collection.json)** - Ready-to-use API collection
  - 40 pre-configured requests
  - Automatic token management
  - Environment variables
  - Test scripts
  - Response validation

## Documentation Statistics

- **Total Documentation**: ~7,300 lines
- **API Endpoints Documented**: 39 endpoints
- **Postman Requests**: 40 requests across 6 categories
- **Code Examples**: 100+ examples
- **Diagrams**: Architecture, data flow, and multi-tenancy diagrams

## File Sizes

| File | Size | Lines | Description |
|------|------|-------|-------------|
| API_REFERENCE.md | 54 KB | 2,618 | Complete API documentation |
| ARCHITECTURE_GUIDE.md | 44 KB | 1,430 | Architecture deep dive |
| GETTING_STARTED.md | 21 KB | 943 | Quick start and workflows |
| GenericAuth.postman_collection.json | 36 KB | - | Postman collection |

## What's Documented

### Authentication Endpoints (6)
- User registration
- Login (Auth Admin and Regular User)
- Token refresh
- Logout
- Password reset flow

### User Management (7)
- List users (paginated)
- Get user by ID
- Update user
- Activate/deactivate user
- Assign/remove system roles

### Application Management (2)
- Create application with initial roles
- Get application by code

### Application Roles (10)
- List application roles
- CRUD operations
- Activate/deactivate
- Set default role
- Permission management

### System Roles (9)
- List system roles
- CRUD operations
- Activate/deactivate
- Permission management

### User-Application Assignments (5)
- Assign user to application
- List user's applications
- List application's users
- Change user role
- Remove user from application

## How to Use This Documentation

### For Developers Getting Started
1. Start with **[Getting Started Guide](./GETTING_STARTED.md)**
2. Follow the Quick Start section
3. Use the Postman collection for hands-on testing
4. Reference the API Reference as needed

### For API Integration
1. Read **[API Reference](./API_REFERENCE.md)**
2. Import the Postman collection
3. Test endpoints in Swagger UI (http://localhost:5000)
4. Implement using provided cURL/code examples

### For Architecture Understanding
1. Read **[Architecture Guide](./ARCHITECTURE_GUIDE.md)**
2. Study the Clean Architecture layers
3. Understand multi-tenant design
4. Learn about design patterns used

### For Testing
1. Import **[Postman Collection](./GenericAuth.postman_collection.json)**
2. Set environment variables (base_url, etc.)
3. Run "Login as Auth Admin" to get token
4. Execute other requests automatically authenticated

## Key Features Documented

### Multi-Tenant Architecture
- Application-scoped roles for tenant isolation
- Dual user types (Auth Admin vs Regular User)
- JWT tokens with application context
- User-application assignments

### Security
- JWT Bearer authentication
- PBKDF2 password hashing
- Refresh token rotation
- Authorization policies
- API key authentication for applications

### Development
- Clean Architecture with DDD
- CQRS with MediatR
- Repository and Unit of Work patterns
- Result pattern for error handling
- Comprehensive validation

## Additional Resources

### Project Documentation
- **[Main README](../README.md)** - Project overview and setup
- **[Testing Guide](../TESTING_GUIDE.md)** - Test workflows and scenarios
- **[Architecture](../ARCHITECTURE.md)** - Original architectural decisions

### Development
- **[Git Workflow](../.github/GIT_WORKFLOW.md)** - Branching strategy
- **[CI/CD Pipeline](../.github/CICD_DOCUMENTATION.md)** - Build and deployment
- **[Pull Request Template](../.github/pull_request_template.md)** - PR guidelines

### Live Documentation
- **Swagger UI**: http://localhost:5000 (when running)
- **Health Check**: http://localhost:5000/health

## Contributing to Documentation

When adding or updating documentation:

1. **API Reference**: Update endpoint documentation with examples
2. **Getting Started**: Add common workflows and troubleshooting
3. **Architecture**: Document design decisions and patterns
4. **Postman**: Add new requests with test scripts

### Documentation Standards
- Use markdown formatting
- Include code examples with syntax highlighting
- Provide both success and error scenarios
- Add cURL examples for API endpoints
- Keep examples realistic and tested
- Update file sizes in this README

## Version History

### Version 1.0 (Current)
- Complete API reference for all endpoints
- Comprehensive getting started guide
- Architecture documentation
- Postman collection with 40 requests
- Updated project README

---

**Last Updated**: November 9, 2025
**Documentation Version**: 1.0
**API Version**: v1
