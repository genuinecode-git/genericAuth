# GenericAuth to Heimdallr - Comprehensive Rename Plan

**Project**: GenericAuth .NET 8.0 Clean Architecture API
**New Name**: Heimdallr (Norse mythology - guardian of the gods, keeper of the Bifrost bridge)
**Date**: 2025-11-12
**Version**: 1.0.1.0 (current)

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Scope Analysis](#2-scope-analysis)
3. [Pre-Rename Checklist](#3-pre-rename-checklist)
4. [Rename Strategy](#4-rename-strategy)
5. [Technical Implementation Plan](#5-technical-implementation-plan)
6. [File & Folder Rename Map](#6-file--folder-rename-map)
7. [Namespace Transformation Guide](#7-namespace-transformation-guide)
8. [Configuration Update Guide](#8-configuration-update-guide)
9. [Database Considerations](#9-database-considerations)
10. [Breaking Changes Assessment](#10-breaking-changes-assessment)
11. [Testing Strategy](#11-testing-strategy)
12. [Automated vs Manual Renaming](#12-automated-vs-manual-renaming)
13. [Step-by-Step Execution Plan](#13-step-by-step-execution-plan)
14. [Post-Rename Verification Checklist](#14-post-rename-verification-checklist)
15. [Rollback Plan](#15-rollback-plan)
16. [Risk Assessment](#16-risk-assessment)
17. [Domain & Branding Considerations](#17-domain--branding-considerations)
18. [Estimated Time & Complexity](#18-estimated-time--complexity)

---

## 1. Executive Summary

### What We're Renaming
- **Solution Name**: GenericAuth.sln → Heimdallr.sln
- **4 Project Assemblies**: GenericAuth.{API,Application,Infrastructure,Domain} → Heimdallr.*
- **4 Test Projects**: GenericAuth.*.{UnitTests,IntegrationTests} → Heimdallr.*
- **All Namespaces**: GenericAuth.* → Heimdallr.*
- **Configuration Values**: JWT Issuer/Audience, logging, database names
- **Docker Images**: genericauth-api → heimdallr-api
- **Kubernetes Resources**: genericauth namespace → heimdallr
- **Documentation**: All references in 15+ markdown files

### Complexity Level
**HIGH** - This is a comprehensive rename affecting:
- 8 .csproj files
- ~100+ .cs source files
- ~20+ configuration files
- Docker & Kubernetes deployments
- GitHub Actions workflows
- Extensive documentation

### Recommended Approach
**Single Large Commit** on a dedicated feature branch with comprehensive testing before merge.

### Estimated Time
- **Preparation**: 30 minutes
- **Execution**: 2-3 hours
- **Testing**: 1-2 hours
- **Total**: 4-6 hours

---

## 2. Scope Analysis

### 2.1 Complete Inventory

#### Source Code (8 Projects)
```
src/GenericAuth.API/                     → src/Heimdallr.API/
src/GenericAuth.Application/             → src/Heimdallr.Application/
src/GenericAuth.Infrastructure/          → src/Heimdallr.Infrastructure/
src/GenericAuth.Domain/                  → src/Heimdallr.Domain/
tests/GenericAuth.API.IntegrationTests/  → tests/Heimdallr.API.IntegrationTests/
tests/GenericAuth.Application.UnitTests/ → tests/Heimdallr.Application.UnitTests/
tests/GenericAuth.Domain.UnitTests/      → tests/Heimdallr.Domain.UnitTests/
tests/GenericAuth.Infrastructure.IntegrationTests/ → tests/Heimdallr.Infrastructure.IntegrationTests/
```

#### Solution & Project Files
- `GenericAuth.sln` → `Heimdallr.sln`
- `GenericAuth.*.csproj` (8 files) → `Heimdallr.*.csproj`

#### Namespaces (Found ~20 unique namespace declarations)
```csharp
namespace GenericAuth.Domain.*         → namespace Heimdallr.Domain.*
namespace GenericAuth.Application.*    → namespace Heimdallr.Application.*
namespace GenericAuth.Infrastructure.* → namespace Heimdallr.Infrastructure.*
namespace GenericAuth.API.*            → namespace Heimdallr.API.*
```

#### Using Statements (Found ~43 occurrences)
```csharp
using GenericAuth.Domain.*;
using GenericAuth.Application.*;
using GenericAuth.Infrastructure.*;
using GenericAuth.API.*;
```

#### Configuration Files
1. `appsettings.json` - Logging, JWT Issuer/Audience
2. `appsettings.Development.json` - Connection strings
3. `launchSettings.json` - Project names
4. `Dockerfile` - Image name, assembly name
5. `docker-compose.yml` - Service names, container names, image names, volumes, networks
6. `docker-stack.yml` - Same as docker-compose
7. `.dockerignore` - No changes needed
8. `.env.template` - Database names, variable values

#### Kubernetes (2 files)
1. `kubernetes/deployment.yaml` - Namespace, ConfigMap, Secrets, Deployment, Service, HPA
2. `kubernetes/ingress.yaml` - Ingress rules

#### GitHub Actions Workflows (2 files)
1. `.github/workflows/ci-cd.yml` - Solution path, test project paths
2. `.github/workflows/release.yml` - Solution path, docker image name, release notes

#### Documentation (15+ files)
1. `README.md` - Main project documentation
2. `ARCHITECTURE.md` - Architecture documentation
3. `TESTING_GUIDE.md` - Testing documentation
4. `IMPLEMENTATION_*.md` (4 files) - Implementation documentation
5. `MULTI_TENANT_IMPLEMENTATION.md` - Multi-tenant documentation
6. `docs/API_REFERENCE.md` - API documentation
7. `docs/GETTING_STARTED.md` - Getting started guide
8. `docs/ARCHITECTURE_GUIDE.md` - Architecture guide
9. `docs/CONTAINERIZATION_*.md` (3 files) - Docker documentation
10. `docs/DOCKER_QUICKSTART.md` - Docker quickstart
11. `docs/README.md` - Docs index
12. `docs/GenericAuth.postman_collection.json` - Postman collection
13. `tests/GenericAuth.API.IntegrationTests/README.md` - Test documentation
14. `tests/GenericAuth.API.IntegrationTests/SETUP_GUIDE.md` - Test setup
15. `agents/MULTI_TENANT_ARCHITECTURE_PLAN.md` - Agent documentation

#### Database-Related
- **Database Files**: `GenericAuth.db` (SQLite) → `Heimdallr.db`
- **Connection Strings**: References to "genericauth" database name
- **Migrations**: Keep existing migrations (historical record)
- **DbContext**: `ApplicationDbContext` (keep as-is, not project-specific)

#### Other Files
1. `VERSION` - Version file (no changes)
2. `.gitignore` - No changes needed
3. `.DS_Store` - No changes needed
4. `nginx/nginx.conf` - Upstream names, server names
5. `scripts/*` - Any script files with references

---

## 3. Pre-Rename Checklist

### 3.1 Create Backup
```bash
# Tag current state
git tag pre-heimdallr-rename
git push origin pre-heimdallr-rename

# Or create a full backup
cd /Users/prabathsl/Projects/GenericAuth
tar -czf genericAuth-backup-$(date +%Y%m%d-%H%M%S).tar.gz genericAuth/
```

### 3.2 Verify Clean State
```bash
cd /Users/prabathsl/Projects/GenericAuth/genericAuth
git status
# Should show only the uncommitted migrations and changes noted in git status
```

### 3.3 Commit Pending Changes
```bash
# Commit the current modified files (migrations, config changes)
git add .
git commit -m "chore: Commit pending changes before Heimdallr rename"
git push origin development
```

### 3.4 Create Feature Branch
```bash
git checkout -b feature/rename-to-heimdallr
```

### 3.5 Verify Build Success
```bash
dotnet clean
dotnet restore GenericAuth.sln
dotnet build GenericAuth.sln --configuration Release
```

### 3.6 Run All Tests (Baseline)
```bash
dotnet test GenericAuth.sln --configuration Release
# Record results - all should pass
```

### 3.7 Document Current State
```bash
# Save current project structure
find . -name "*.csproj" -o -name "*.sln" > pre-rename-project-files.txt

# Save current namespace count
grep -r "namespace GenericAuth" src/ tests/ | wc -l > pre-rename-namespace-count.txt
```

---

## 4. Rename Strategy

### 4.1 Order of Operations

**Critical Sequence** (to avoid breaking dependencies):

1. **Phase 1: Folder & File Renames** (Git-tracked moves)
   - Use `git mv` to preserve history
   - Rename solution file first
   - Rename project folders
   - Rename .csproj files
   - Rename test project folders and files

2. **Phase 2: Solution File Updates**
   - Update .sln file project references
   - Update project GUIDs (if needed)

3. **Phase 3: Project File Updates**
   - Update <RootNamespace> in .csproj
   - Update <AssemblyName> in .csproj
   - Update <ProjectReference> paths in .csproj

4. **Phase 4: Source Code Namespace Updates**
   - Update namespace declarations in .cs files
   - Update using statements in .cs files
   - Use IDE refactoring or regex find/replace

5. **Phase 5: Configuration File Updates**
   - appsettings.json (JWT, logging)
   - launchSettings.json
   - Docker files
   - Kubernetes files
   - GitHub workflows

6. **Phase 6: Documentation Updates**
   - README.md and all markdown files
   - Postman collection
   - Code comments (selective)

7. **Phase 7: Database & Migration Updates**
   - Update connection strings
   - Consider database name changes

### 4.2 Git Considerations

**Use `git mv` for all folder/file renames** to preserve history:
```bash
git mv src/GenericAuth.API src/Heimdallr.API
```

**Advantages**:
- Git tracks the rename in history
- `git log --follow` works correctly
- Blame/history preserved for all files

**Disadvantages**:
- Slightly more verbose than IDE refactoring

**Recommendation**: Use `git mv` for all structural changes, then use IDE/tools for content updates.

---

## 5. Technical Implementation Plan

### 5.1 Backend Renaming

#### A. Solution File
```xml
<!-- GenericAuth.sln → Heimdallr.sln -->
<!-- Update all project paths -->
Before: src\GenericAuth.Domain\GenericAuth.Domain.csproj
After:  src\Heimdallr.Domain\Heimdallr.Domain.csproj
```

#### B. Project Files (RootNamespace & AssemblyName)
```xml
<!-- In each .csproj -->
<PropertyGroup>
  <RootNamespace>Heimdallr.API</RootNamespace>
  <AssemblyName>Heimdallr.API</AssemblyName>
</PropertyGroup>
```

#### C. Project References
```xml
<!-- Update all <ProjectReference> paths -->
Before: <ProjectReference Include="..\GenericAuth.Application\GenericAuth.Application.csproj" />
After:  <ProjectReference Include="..\Heimdallr.Application\Heimdallr.Application.csproj" />
```

#### D. Namespace Declarations
```csharp
// Before
namespace GenericAuth.Domain.Entities;

// After
namespace Heimdallr.Domain.Entities;
```

#### E. Using Statements
```csharp
// Before
using GenericAuth.Domain.Entities;
using GenericAuth.Application.Common.Interfaces;

// After
using Heimdallr.Domain.Entities;
using Heimdallr.Application.Common.Interfaces;
```

### 5.2 Frontend Renaming (For Future Reference)

When the React frontend is developed:

```typescript
// Package names
@genericauth/sdk → @heimdallr/sdk
@genericauth/components → @heimdallr/components

// API client
import { GenericAuthClient } from '@genericauth/sdk';
→
import { HeimdallrClient } from '@heimdallr/sdk';

// Domain names
auth.genericauth.com → auth.heimdallr.io
oauth.genericauth.com → oauth.heimdallr.io
admin.genericauth.com → admin.heimdallr.io
```

---

## 6. File & Folder Rename Map

### 6.1 Solution & Project Files

| Old Path | New Path | Method |
|----------|----------|--------|
| `GenericAuth.sln` | `Heimdallr.sln` | `git mv` |
| `src/GenericAuth.API/GenericAuth.API.csproj` | `src/Heimdallr.API/Heimdallr.API.csproj` | `git mv` |
| `src/GenericAuth.Application/GenericAuth.Application.csproj` | `src/Heimdallr.Application/Heimdallr.Application.csproj` | `git mv` |
| `src/GenericAuth.Infrastructure/GenericAuth.Infrastructure.csproj` | `src/Heimdallr.Infrastructure/Heimdallr.Infrastructure.csproj` | `git mv` |
| `src/GenericAuth.Domain/GenericAuth.Domain.csproj` | `src/Heimdallr.Domain/Heimdallr.Domain.csproj` | `git mv` |
| `tests/GenericAuth.API.IntegrationTests/GenericAuth.API.IntegrationTests.csproj` | `tests/Heimdallr.API.IntegrationTests/Heimdallr.API.IntegrationTests.csproj` | `git mv` |
| `tests/GenericAuth.Application.UnitTests/GenericAuth.Application.UnitTests.csproj` | `tests/Heimdallr.Application.UnitTests/Heimdallr.Application.UnitTests.csproj` | `git mv` |
| `tests/GenericAuth.Domain.UnitTests/GenericAuth.Domain.UnitTests.csproj` | `tests/Heimdallr.Domain.UnitTests/Heimdallr.Domain.UnitTests.csproj` | `git mv` |
| `tests/GenericAuth.Infrastructure.IntegrationTests/GenericAuth.Infrastructure.IntegrationTests.csproj` | `tests/Heimdallr.Infrastructure.IntegrationTests/Heimdallr.Infrastructure.IntegrationTests.csproj` | `git mv` |

### 6.2 Project Folders

| Old Folder | New Folder | Method |
|------------|------------|--------|
| `src/GenericAuth.API/` | `src/Heimdallr.API/` | `git mv` |
| `src/GenericAuth.Application/` | `src/Heimdallr.Application/` | `git mv` |
| `src/GenericAuth.Infrastructure/` | `src/Heimdallr.Infrastructure/` | `git mv` |
| `src/GenericAuth.Domain/` | `src/Heimdallr.Domain/` | `git mv` |
| `tests/GenericAuth.API.IntegrationTests/` | `tests/Heimdallr.API.IntegrationTests/` | `git mv` |
| `tests/GenericAuth.Application.UnitTests/` | `tests/Heimdallr.Application.UnitTests/` | `git mv` |
| `tests/GenericAuth.Domain.UnitTests/` | `tests/Heimdallr.Domain.UnitTests/` | `git mv` |
| `tests/GenericAuth.Infrastructure.IntegrationTests/` | `tests/Heimdallr.Infrastructure.IntegrationTests/` | `git mv` |

### 6.3 Documentation Files

| Old Path | New Path | Method |
|----------|----------|--------|
| `docs/GenericAuth.postman_collection.json` | `docs/Heimdallr.postman_collection.json` | `git mv` |

### 6.4 Database Files (Runtime)

| Old File | New File | Notes |
|----------|----------|-------|
| `GenericAuth.db` | `Heimdallr.db` | SQLite (Development only) |
| `genericauth` | `heimdallr` | PostgreSQL database name |

---

## 7. Namespace Transformation Guide

### 7.1 Namespace Patterns

| Old Namespace | New Namespace |
|--------------|---------------|
| `GenericAuth.Domain` | `Heimdallr.Domain` |
| `GenericAuth.Domain.Common` | `Heimdallr.Domain.Common` |
| `GenericAuth.Domain.Entities` | `Heimdallr.Domain.Entities` |
| `GenericAuth.Domain.ValueObjects` | `Heimdallr.Domain.ValueObjects` |
| `GenericAuth.Domain.Enums` | `Heimdallr.Domain.Enums` |
| `GenericAuth.Domain.Events` | `Heimdallr.Domain.Events` |
| `GenericAuth.Domain.Exceptions` | `Heimdallr.Domain.Exceptions` |
| `GenericAuth.Domain.Interfaces` | `Heimdallr.Domain.Interfaces` |
| `GenericAuth.Domain.Services` | `Heimdallr.Domain.Services` |
| `GenericAuth.Domain.Aggregates` | `Heimdallr.Domain.Aggregates` |
| `GenericAuth.Application` | `Heimdallr.Application` |
| `GenericAuth.Application.Common` | `Heimdallr.Application.Common` |
| `GenericAuth.Application.Common.Interfaces` | `Heimdallr.Application.Common.Interfaces` |
| `GenericAuth.Application.Features.*` | `Heimdallr.Application.Features.*` |
| `GenericAuth.Infrastructure` | `Heimdallr.Infrastructure` |
| `GenericAuth.Infrastructure.Persistence` | `Heimdallr.Infrastructure.Persistence` |
| `GenericAuth.Infrastructure.Identity` | `Heimdallr.Infrastructure.Identity` |
| `GenericAuth.Infrastructure.Services` | `Heimdallr.Infrastructure.Services` |
| `GenericAuth.API` | `Heimdallr.API` |
| `GenericAuth.API.Controllers` | `Heimdallr.API.Controllers` |
| `GenericAuth.API.Middleware` | `Heimdallr.API.Middleware` |

### 7.2 Regex Patterns for Find/Replace

**Visual Studio / VS Code / Rider:**

```regex
Find:    namespace GenericAuth\.([A-Za-z0-9\.]+)
Replace: namespace Heimdallr.$1

Find:    using GenericAuth\.([A-Za-z0-9\.]+)
Replace: using Heimdallr.$1
```

**Command Line (PowerShell):**
```powershell
# Find all .cs files and replace namespace declarations
Get-ChildItem -Path . -Include *.cs -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName
    $content = $content -replace 'namespace GenericAuth\.', 'namespace Heimdallr.'
    $content = $content -replace 'using GenericAuth\.', 'using Heimdallr.'
    Set-Content -Path $_.FullName -Value $content
}
```

**Command Line (Bash/macOS):**
```bash
# Find all .cs files and replace namespace declarations
find . -name "*.cs" -type f -exec sed -i '' 's/namespace GenericAuth\./namespace Heimdallr./g' {} +
find . -name "*.cs" -type f -exec sed -i '' 's/using GenericAuth\./using Heimdallr./g' {} +
```

---

## 8. Configuration Update Guide

### 8.1 appsettings.json

**File**: `src/Heimdallr.API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Heimdallr": "Information"  // ← Change from "GenericAuth"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Heimdallr.db"  // ← Change from "GenericAuth.db"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "Heimdallr",  // ← Change from "GenericAuth"
    "Audience": "HeimdallrClient",  // ← Change from "GenericAuthClient"
    "ExpirationInMinutes": "60"
  }
}
```

### 8.2 launchSettings.json

**File**: `src/Heimdallr.API/Properties/launchSettings.json`

No changes needed - only contains port configurations.

### 8.3 Dockerfile

**File**: `Dockerfile`

```dockerfile
# Update comments and image name
# =================================================================
# Heimdallr API - Multi-Stage Dockerfile
# Optimized for .NET 8.0 LTS with Alpine Linux
# =================================================================

# Copy solution and project files
COPY ["Heimdallr.sln", "."]
COPY ["src/Heimdallr.API/Heimdallr.API.csproj", "src/Heimdallr.API/"]
COPY ["src/Heimdallr.Application/Heimdallr.Application.csproj", "src/Heimdallr.Application/"]
COPY ["src/Heimdallr.Infrastructure/Heimdallr.Infrastructure.csproj", "src/Heimdallr.Infrastructure/"]
COPY ["src/Heimdallr.Domain/Heimdallr.Domain.csproj", "src/Heimdallr.Domain/"]

# Restore dependencies
RUN dotnet restore "Heimdallr.sln"

# Build
WORKDIR "/src/src/Heimdallr.API"
RUN dotnet build "Heimdallr.API.csproj" ...

# Publish
RUN dotnet publish "Heimdallr.API.csproj" ...

# Entry point
ENTRYPOINT ["dotnet", "Heimdallr.API.dll"]
```

### 8.4 docker-compose.yml

**File**: `docker-compose.yml`

```yaml
# =================================================================
# Heimdallr - Docker Compose Configuration
# =================================================================

services:
  postgres:
    container_name: heimdallr-postgres
    environment:
      POSTGRES_DB: ${DB_NAME:-heimdallr}
      POSTGRES_USER: ${DB_USER:-heimdallr}
      # ...
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER:-heimdallr}"]
    networks:
      - heimdallr-network

  redis:
    container_name: heimdallr-redis
    networks:
      - heimdallr-network

  seq:
    container_name: heimdallr-seq
    networks:
      - heimdallr-network

  api:
    image: heimdallr-api:latest
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=${DB_NAME:-heimdallr};Username=${DB_USER:-heimdallr};Password=${DB_PASSWORD:-Dev@Pass123};...
      - JwtSettings__Issuer=${JWT_ISSUER:-Heimdallr}
      - JwtSettings__Audience=${JWT_AUDIENCE:-HeimdallrClient}
    networks:
      - heimdallr-network

  nginx:
    container_name: heimdallr-nginx
    networks:
      - heimdallr-network

volumes:
  postgres-data:
    name: heimdallr-postgres-data
  redis-data:
    name: heimdallr-redis-data
  seq-data:
    name: heimdallr-seq-data

networks:
  heimdallr-network:
    name: heimdallr-network
```

### 8.5 .env.template

**File**: `.env.template`

```bash
# Database Configuration
DB_NAME=heimdallr
DB_USER=heimdallr
DB_PASSWORD=Dev@Pass123

# JWT Configuration
JWT_ISSUER=Heimdallr
JWT_AUDIENCE=HeimdallrClient
```

### 8.6 Kubernetes Files

**File**: `kubernetes/deployment.yaml`

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: heimdallr
  labels:
    name: heimdallr

---
apiVersion: v1
kind: ConfigMap
metadata:
  name: heimdallr-config
  namespace: heimdallr
data:
  JwtSettings__Issuer: "Heimdallr"
  JwtSettings__Audience: "HeimdallrClient"

---
apiVersion: v1
kind: Secret
metadata:
  name: heimdallr-secrets
  namespace: heimdallr
stringData:
  db-connection-string: "Host=postgres-service;Database=heimdallr;Username=heimdallr;..."

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: heimdallr-api
  namespace: heimdallr
  labels:
    app: heimdallr-api
spec:
  selector:
    matchLabels:
      app: heimdallr-api
  template:
    metadata:
      labels:
        app: heimdallr-api
    spec:
      containers:
      - name: api
        image: heimdallr-api:latest

---
apiVersion: v1
kind: Service
metadata:
  name: heimdallr-api-service
  namespace: heimdallr
  labels:
    app: heimdallr-api
spec:
  selector:
    app: heimdallr-api

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: heimdallr-api-hpa
  namespace: heimdallr
spec:
  scaleTargetRef:
    name: heimdallr-api

---
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: heimdallr-api-pdb
  namespace: heimdallr
spec:
  selector:
    matchLabels:
      app: heimdallr-api

---
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: heimdallr-api-network-policy
  namespace: heimdallr
spec:
  podSelector:
    matchLabels:
      app: heimdallr-api
```

**File**: `kubernetes/ingress.yaml`

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: heimdallr-ingress
  namespace: heimdallr
spec:
  rules:
  - host: api.heimdallr.io  # Update host
    http:
      paths:
      - path: /
        backend:
          service:
            name: heimdallr-api-service
```

### 8.7 GitHub Actions Workflows

**File**: `.github/workflows/ci-cd.yml`

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  SOLUTION_PATH: 'Heimdallr.sln'  # ← Change

jobs:
  build-test:
    steps:
    - name: Run Domain Unit Tests
      run: |
        dotnet test tests/Heimdallr.Domain.UnitTests/Heimdallr.Domain.UnitTests.csproj ...

    - name: Run Application Unit Tests
      run: |
        dotnet test tests/Heimdallr.Application.UnitTests/Heimdallr.Application.UnitTests.csproj ...

    - name: Run Infrastructure Integration Tests
      run: |
        dotnet test tests/Heimdallr.Infrastructure.IntegrationTests/Heimdallr.Infrastructure.IntegrationTests.csproj ...

    - name: Run API Integration Tests
      run: |
        dotnet test tests/Heimdallr.API.IntegrationTests/Heimdallr.API.IntegrationTests.csproj ...
```

**File**: `.github/workflows/release.yml`

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  SOLUTION_PATH: 'Heimdallr.sln'  # ← Change
  DOCKER_IMAGE_NAME: 'heimdallr-api'  # ← Change

jobs:
  build-test-release:
    steps:
    - name: Generate release notes
      run: |
        cat > release/RELEASE_NOTES.md << EOF
        # Heimdallr API Release v${{ steps.increment_version.outputs.new_version }}
        ...
        \`\`\`bash
        docker run -d -p 8080:8080 -p 8081:8081 \\
          --name heimdallr-api \\
          ${{ env.DOCKER_IMAGE_NAME }}:${{ steps.increment_version.outputs.new_version }}
        \`\`\`
        EOF
```

---

## 9. Database Considerations

### 9.1 Database Context Name

**Recommendation**: Keep `ApplicationDbContext` as-is.

**Reasoning**:
- DbContext names are typically generic (ApplicationDbContext, AppDbContext, etc.)
- Not project-specific
- Changing would add unnecessary complexity
- No external references to this name

### 9.2 Existing Migrations

**Recommendation**: Keep all existing migrations unchanged.

**Reasoning**:
- Migrations are historical records
- Already applied to existing databases
- Changing migration names breaks `__EFMigrationsHistory` table
- New migrations will use new namespace automatically

**Result**: Migration files will have mixed namespaces:
```csharp
// Old migration (unchanged)
namespace GenericAuth.Infrastructure.Migrations;
public partial class InitialCreate : Migration { }

// New migration (after rename)
namespace Heimdallr.Infrastructure.Migrations;
public partial class AddNewFeature : Migration { }
```

This is **acceptable and common** in projects that undergo renames.

### 9.3 Connection Strings

**SQLite (Development)**:
```json
// Before
"DefaultConnection": "Data Source=GenericAuth.db"

// After
"DefaultConnection": "Data Source=Heimdallr.db"
```

**PostgreSQL (Production)**:
```bash
# Before
POSTGRES_DB=genericauth
DB_NAME=genericauth

# After
POSTGRES_DB=heimdallr
DB_NAME=heimdallr
```

**Impact**: This creates a NEW database. Existing data migration required if needed.

### 9.4 Schema Names

No schema prefixes currently used (default `dbo` or `public`). No changes needed.

---

## 10. Breaking Changes Assessment

### 10.1 API Breaking Changes

#### JWT Tokens (HIGH IMPACT)
**Issuer and Audience Claims Changed**:
```json
// Before
{
  "iss": "GenericAuth",
  "aud": "GenericAuthClient"
}

// After
{
  "iss": "Heimdallr",
  "aud": "HeimdallrClient"
}
```

**Impact**:
- All existing JWT tokens become INVALID immediately
- All users must re-authenticate
- Refresh tokens become invalid

**Mitigation**:
- Announce token invalidation in release notes
- Consider temporary dual validation (accept both old and new issuer/audience)

#### Database Name Change
**Before**: `genericauth` database
**After**: `heimdallr` database

**Impact**:
- Creates a new empty database
- Existing data not migrated
- All user accounts, roles, permissions need re-creation OR data migration

**Mitigation Options**:
1. **Fresh Start**: Acceptable for pre-production/development
2. **Data Migration**: Export from old DB, import to new DB
3. **Keep Database Name**: Use `genericauth` database even with Heimdallr code

**Recommendation for Production**: Keep database name as `genericauth` initially, rename later with proper migration.

### 10.2 Docker Breaking Changes

**Image Name Change**:
- Old: `genericauth-api:latest`
- New: `heimdallr-api:latest`

**Impact**:
- Existing deployments must pull new image
- Old image references in scripts/compose files break

**Volume Names**:
- Old: `genericauth-postgres-data`, `genericauth-redis-data`
- New: `heimdallr-postgres-data`, `heimdallr-redis-data`

**Impact**:
- New volumes created (data not migrated)
- Old volumes orphaned

**Mitigation**:
- Document volume migration steps
- Provide migration script

### 10.3 Kubernetes Breaking Changes

**Namespace Change**:
- Old: `genericauth`
- New: `heimdallr`

**Impact**:
- Complete redeployment required
- All resources (pods, services, secrets) recreated
- Downtime during migration

**Service Names**:
- Old: `genericauth-api-service`
- New: `heimdallr-api-service`

**Impact**:
- DNS names change
- Internal service discovery breaks

### 10.4 Existing User Impact

**For Development**:
- Default admin credentials still work (email `admin@genericauth.com`)
- Database seeding recreates default user
- No migration needed

**For Production**:
- All tokens invalidated (planned re-authentication)
- Database migration required OR fresh start
- API endpoint URLs may change (if domain changes)

### 10.5 CI/CD Impact

**GitHub Actions**:
- Workflow files updated
- Test paths updated
- No breaking changes (internal)

**Artifacts**:
- New artifact names
- Old artifacts remain available

---

## 11. Testing Strategy

### 11.1 Build Verification

```bash
# Clean build
dotnet clean Heimdallr.sln
rm -rf **/bin **/obj

# Restore
dotnet restore Heimdallr.sln

# Build
dotnet build Heimdallr.sln --configuration Release

# Expected: Success with 0 errors
```

### 11.2 Unit & Integration Tests

```bash
# Run all tests
dotnet test Heimdallr.sln --configuration Release --verbosity normal

# Run with coverage
dotnet test Heimdallr.sln \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --verbosity normal

# Expected: All tests pass (same count as before rename)
```

### 11.3 Runtime Testing

```bash
# Run API
cd src/Heimdallr.API
dotnet run

# Expected:
# - API starts successfully
# - Migrations apply automatically
# - Database seeded with default admin
# - Swagger UI accessible
```

### 11.4 Docker Build Testing

```bash
# Build image
docker build -t heimdallr-api:test -f Dockerfile .

# Run container
docker run -d -p 8080:8080 -p 8081:8081 \
  --name heimdallr-api-test \
  heimdallr-api:test

# Test health endpoint
curl http://localhost:8080/health

# Expected: {"status": "Healthy"}

# Cleanup
docker stop heimdallr-api-test
docker rm heimdallr-api-test
```

### 11.5 Docker Compose Testing

```bash
# Build and start services
docker-compose up --build -d

# Check service health
docker-compose ps

# Test API through NGINX
curl http://localhost/health

# Cleanup
docker-compose down -v
```

### 11.6 Manual Testing Checklist

- [ ] Login with default admin credentials
- [ ] Create new user
- [ ] Create new application
- [ ] Assign role to user
- [ ] Test JWT token validation
- [ ] Test refresh token flow
- [ ] Test password reset flow
- [ ] Verify database migrations applied
- [ ] Check logs for errors
- [ ] Verify Swagger UI works

### 11.7 Automated Testing

Run CI/CD workflow locally:
```bash
# Install act (GitHub Actions local runner)
brew install act  # macOS

# Run workflow
act -j build-test
```

---

## 12. Automated vs Manual Renaming

### 12.1 Use IDE Refactoring For

**Visual Studio / Rider / VS Code**:
- ✅ Namespace renames (right-click → Rename)
- ✅ Class renames
- ✅ Using statement updates (automatic)
- ✅ Project references (automatic after file rename)

**Advantages**:
- Updates all references automatically
- Preserves code correctness
- Faster than manual editing

**Recommended for**: Source code namespace changes

### 12.2 Use Command-Line Tools For

**Find/Replace (sed, PowerShell)**:
- ✅ Configuration files (.json, .yml, .yaml)
- ✅ Documentation (.md)
- ✅ Comments and strings
- ✅ Bulk text replacements

**Recommended for**: Non-code files

### 12.3 Manual Editing For

- ✅ Solution file (.sln) - complex structure
- ✅ .csproj files - critical structure
- ✅ Selective documentation updates
- ✅ Reviewing automated changes

---

## 13. Step-by-Step Execution Plan

### Phase 1: Preparation (15 minutes)

```bash
# 1. Verify clean state
cd /Users/prabathsl/Projects/GenericAuth/genericAuth
git status

# 2. Commit any pending changes
git add .
git commit -m "chore: Commit pending changes before Heimdallr rename"
git push origin development

# 3. Create backup tag
git tag pre-heimdallr-rename-$(date +%Y%m%d)
git push origin --tags

# 4. Create feature branch
git checkout -b feature/rename-to-heimdallr

# 5. Run baseline tests
dotnet test GenericAuth.sln --configuration Release > test-results-before.txt
```

### Phase 2: Folder & File Renames (30 minutes)

```bash
# 1. Rename solution file
git mv GenericAuth.sln Heimdallr.sln

# 2. Rename project folders and .csproj files
# Note: Rename folder first, then the .csproj inside

# Domain
git mv src/GenericAuth.Domain src/Heimdallr.Domain
git mv src/Heimdallr.Domain/GenericAuth.Domain.csproj src/Heimdallr.Domain/Heimdallr.Domain.csproj

# Application
git mv src/GenericAuth.Application src/Heimdallr.Application
git mv src/Heimdallr.Application/GenericAuth.Application.csproj src/Heimdallr.Application/Heimdallr.Application.csproj

# Infrastructure
git mv src/GenericAuth.Infrastructure src/Heimdallr.Infrastructure
git mv src/Heimdallr.Infrastructure/GenericAuth.Infrastructure.csproj src/Heimdallr.Infrastructure/Heimdallr.Infrastructure.csproj

# API
git mv src/GenericAuth.API src/Heimdallr.API
git mv src/Heimdallr.API/GenericAuth.API.csproj src/Heimdallr.API/Heimdallr.API.csproj

# 3. Rename test project folders and .csproj files

# Domain Unit Tests
git mv tests/GenericAuth.Domain.UnitTests tests/Heimdallr.Domain.UnitTests
git mv tests/Heimdallr.Domain.UnitTests/GenericAuth.Domain.UnitTests.csproj tests/Heimdallr.Domain.UnitTests/Heimdallr.Domain.UnitTests.csproj

# Application Unit Tests
git mv tests/GenericAuth.Application.UnitTests tests/Heimdallr.Application.UnitTests
git mv tests/Heimdallr.Application.UnitTests/GenericAuth.Application.UnitTests.csproj tests/Heimdallr.Application.UnitTests/Heimdallr.Application.UnitTests.csproj

# Infrastructure Integration Tests
git mv tests/GenericAuth.Infrastructure.IntegrationTests tests/Heimdallr.Infrastructure.IntegrationTests
git mv tests/Heimdallr.Infrastructure.IntegrationTests/GenericAuth.Infrastructure.IntegrationTests.csproj tests/Heimdallr.Infrastructure.IntegrationTests/Heimdallr.Infrastructure.IntegrationTests.csproj

# API Integration Tests
git mv tests/GenericAuth.API.IntegrationTests tests/Heimdallr.API.IntegrationTests
git mv tests/Heimdallr.API.IntegrationTests/GenericAuth.API.IntegrationTests.csproj tests/Heimdallr.API.IntegrationTests/Heimdallr.API.IntegrationTests.csproj

# 4. Rename documentation
git mv docs/GenericAuth.postman_collection.json docs/Heimdallr.postman_collection.json

# 5. Commit renames
git commit -m "refactor: Rename project folders and files from GenericAuth to Heimdallr"
```

### Phase 3: Solution File Update (10 minutes)

```bash
# Edit Heimdallr.sln manually or with script
# Replace all occurrences of:
#   GenericAuth.Domain → Heimdallr.Domain
#   GenericAuth.Application → Heimdallr.Application
#   GenericAuth.Infrastructure → Heimdallr.Infrastructure
#   GenericAuth.API → Heimdallr.API

# macOS/Linux
sed -i '' 's/GenericAuth\.Domain/Heimdallr.Domain/g' Heimdallr.sln
sed -i '' 's/GenericAuth\.Application/Heimdallr.Application/g' Heimdallr.sln
sed -i '' 's/GenericAuth\.Infrastructure/Heimdallr.Infrastructure/g' Heimdallr.sln
sed -i '' 's/GenericAuth\.API/Heimdallr.API/g' Heimdallr.sln

# Verify solution loads
dotnet sln Heimdallr.sln list

# Commit
git add Heimdallr.sln
git commit -m "refactor: Update solution file references to Heimdallr"
```

### Phase 4: Project File Updates (15 minutes)

Update each .csproj file:

```bash
# Update RootNamespace and AssemblyName in all .csproj files
# Can use sed or edit manually

# Example for one file:
# Before:
# <RootNamespace>GenericAuth.API</RootNamespace>
# <AssemblyName>GenericAuth.API</AssemblyName>

# After:
# <RootNamespace>Heimdallr.API</RootNamespace>
# <AssemblyName>Heimdallr.API</AssemblyName>

# Automated approach (macOS):
find src tests -name "*.csproj" -type f -exec sed -i '' 's/<RootNamespace>GenericAuth\./<RootNamespace>Heimdallr./g' {} +
find src tests -name "*.csproj" -type f -exec sed -i '' 's/<AssemblyName>GenericAuth\./<AssemblyName>Heimdallr./g' {} +

# Update ProjectReference paths
find src tests -name "*.csproj" -type f -exec sed -i '' 's/GenericAuth\./Heimdallr./g' {} +

# Verify no errors
dotnet restore Heimdallr.sln
dotnet build Heimdallr.sln --no-restore --configuration Release

# Commit
git add src/ tests/
git commit -m "refactor: Update project file namespaces and references to Heimdallr"
```

### Phase 5: Source Code Namespace Updates (45 minutes)

**Recommended**: Use IDE refactoring for accuracy.

**Visual Studio / Rider**:
1. Open `Heimdallr.sln`
2. Find in Files: `namespace GenericAuth.`
3. Replace with: `namespace Heimdallr.`
4. Review each change
5. Find in Files: `using GenericAuth.`
6. Replace with: `using Heimdallr.`
7. Review each change

**Alternatively (Command Line)**:
```bash
# macOS/Linux
find src tests -name "*.cs" -type f -exec sed -i '' 's/namespace GenericAuth\./namespace Heimdallr./g' {} +
find src tests -name "*.cs" -type f -exec sed -i '' 's/using GenericAuth\./using Heimdallr./g' {} +

# Verify compilation
dotnet build Heimdallr.sln --configuration Release

# Commit
git add src/ tests/
git commit -m "refactor: Update C# namespaces and using statements to Heimdallr"
```

### Phase 6: Configuration Files (30 minutes)

#### 6.1 appsettings.json

```bash
# Edit src/Heimdallr.API/appsettings.json
# Changes:
# - "GenericAuth": "Information" → "Heimdallr": "Information"
# - "Data Source=GenericAuth.db" → "Data Source=Heimdallr.db"
# - "Issuer": "GenericAuth" → "Issuer": "Heimdallr"
# - "Audience": "GenericAuthClient" → "Audience": "HeimdallrClient"

# Manual edit or:
sed -i '' 's/"GenericAuth":/"Heimdallr":/g' src/Heimdallr.API/appsettings.json
sed -i '' 's/GenericAuth\.db/Heimdallr.db/g' src/Heimdallr.API/appsettings.json
sed -i '' 's/"Issuer": "GenericAuth"/"Issuer": "Heimdallr"/g' src/Heimdallr.API/appsettings.json
sed -i '' 's/"Audience": "GenericAuthClient"/"Audience": "HeimdallrClient"/g' src/Heimdallr.API/appsettings.json
```

#### 6.2 Dockerfile

```bash
# Edit Dockerfile
sed -i '' 's/GenericAuth/Heimdallr/g' Dockerfile
```

#### 6.3 docker-compose.yml

```bash
# Edit docker-compose.yml
sed -i '' 's/genericauth/heimdallr/g' docker-compose.yml
sed -i '' 's/GenericAuth/Heimdallr/g' docker-compose.yml
```

#### 6.4 docker-stack.yml

```bash
# Edit docker-stack.yml (if exists)
sed -i '' 's/genericauth/heimdallr/g' docker-stack.yml
sed -i '' 's/GenericAuth/Heimdallr/g' docker-stack.yml
```

#### 6.5 .env.template

```bash
# Edit .env.template
sed -i '' 's/genericauth/heimdallr/g' .env.template
sed -i '' 's/GenericAuth/Heimdallr/g' .env.template
```

#### 6.6 Kubernetes Files

```bash
# Edit kubernetes/deployment.yaml
sed -i '' 's/genericauth/heimdallr/g' kubernetes/deployment.yaml
sed -i '' 's/GenericAuth/Heimdallr/g' kubernetes/deployment.yaml

# Edit kubernetes/ingress.yaml
sed -i '' 's/genericauth/heimdallr/g' kubernetes/ingress.yaml
sed -i '' 's/GenericAuth/Heimdallr/g' kubernetes/ingress.yaml
```

#### 6.7 GitHub Actions

```bash
# Edit .github/workflows/ci-cd.yml
sed -i '' "s/SOLUTION_PATH: 'GenericAuth\.sln'/SOLUTION_PATH: 'Heimdallr.sln'/g" .github/workflows/ci-cd.yml
sed -i '' 's/GenericAuth\./Heimdallr./g' .github/workflows/ci-cd.yml

# Edit .github/workflows/release.yml
sed -i '' "s/SOLUTION_PATH: 'GenericAuth\.sln'/SOLUTION_PATH: 'Heimdallr.sln'/g" .github/workflows/release.yml
sed -i '' "s/DOCKER_IMAGE_NAME: 'genericauth-api'/DOCKER_IMAGE_NAME: 'heimdallr-api'/g" .github/workflows/release.yml
sed -i '' 's/GenericAuth/Heimdallr/g' .github/workflows/release.yml
sed -i '' 's/genericauth/heimdallr/g' .github/workflows/release.yml
```

#### 6.8 Commit Configuration Changes

```bash
git add .
git commit -m "refactor: Update configuration files for Heimdallr rename"
```

### Phase 7: Documentation Updates (30 minutes)

```bash
# Update all markdown files
find . -name "*.md" -not -path "./.git/*" -type f -exec sed -i '' 's/GenericAuth/Heimdallr/g' {} +
find . -name "*.md" -not -path "./.git/*" -type f -exec sed -i '' 's/genericAuth/heimdallr/g' {} +
find . -name "*.md" -not -path "./.git/*" -type f -exec sed -i '' 's/genericauth/heimdallr/g' {} +

# Update Postman collection
sed -i '' 's/GenericAuth/Heimdallr/g' docs/Heimdallr.postman_collection.json

# Manual review recommended for:
# - README.md (check badges, links)
# - ARCHITECTURE.md (architecture-specific references)
# - API_REFERENCE.md (endpoint examples)

# Commit
git add .
git commit -m "docs: Update all documentation for Heimdallr rename"
```

### Phase 8: Build & Test Verification (30 minutes)

```bash
# 1. Clean everything
dotnet clean Heimdallr.sln
rm -rf **/bin **/obj
find . -name "*.db" -delete
find . -name "*.db-shm" -delete
find . -name "*.db-wal" -delete

# 2. Restore
dotnet restore Heimdallr.sln

# 3. Build
dotnet build Heimdallr.sln --configuration Release

# 4. Run all tests
dotnet test Heimdallr.sln --configuration Release --verbosity normal

# 5. Save test results
dotnet test Heimdallr.sln --configuration Release > test-results-after.txt

# 6. Compare test results
echo "Before rename:" && cat test-results-before.txt | grep "Passed"
echo "After rename:" && cat test-results-after.txt | grep "Passed"
```

### Phase 9: Runtime Testing (15 minutes)

```bash
# 1. Run API locally
cd src/Heimdallr.API
dotnet run

# In another terminal:
# 2. Test health endpoint
curl http://localhost:5071/health

# 3. Test Swagger UI
open http://localhost:5071

# 4. Test login with default admin
curl -X POST http://localhost:5071/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@genericauth.com",
    "password": "Admin@123",
    "applicationCode": null
  }'

# Stop the API (Ctrl+C)
cd ../..
```

### Phase 10: Docker Testing (15 minutes)

```bash
# 1. Build Docker image
docker build -t heimdallr-api:test -f Dockerfile .

# 2. Run container
docker run -d \
  -p 8080:8080 \
  -p 8081:8081 \
  --name heimdallr-test \
  heimdallr-api:test

# 3. Test container
sleep 10
curl http://localhost:8080/health

# 4. Check logs
docker logs heimdallr-test

# 5. Cleanup
docker stop heimdallr-test
docker rm heimdallr-test
docker rmi heimdallr-api:test
```

### Phase 11: Final Review & Commit (15 minutes)

```bash
# 1. Review all changes
git status
git diff --name-only pre-heimdallr-rename-$(date +%Y%m%d)..HEAD

# 2. Check for missed references
grep -r "GenericAuth" . \
  --exclude-dir=.git \
  --exclude-dir=bin \
  --exclude-dir=obj \
  --exclude-dir=.vs \
  --exclude-dir=.idea \
  --exclude="*.dll" \
  --exclude="*.exe"

# 3. Create final commit if any loose ends
git add .
git commit -m "refactor: Complete Heimdallr rename - final cleanup"

# 4. Push feature branch
git push origin feature/rename-to-heimdallr
```

### Phase 12: Create Pull Request (10 minutes)

```bash
# 1. Create PR using GitHub CLI (optional)
gh pr create \
  --title "refactor: Rename project from GenericAuth to Heimdallr" \
  --body "$(cat <<EOF
## Summary
Comprehensive rename of the project from GenericAuth to Heimdallr.

## Changes
- ✅ Renamed all project folders and files
- ✅ Updated solution file references
- ✅ Updated all C# namespaces and using statements
- ✅ Updated configuration files (appsettings, Docker, Kubernetes)
- ✅ Updated GitHub Actions workflows
- ✅ Updated all documentation
- ✅ Verified all tests pass
- ✅ Verified Docker build works

## Breaking Changes
- JWT Issuer/Audience changed (all tokens invalidated)
- Database name changed (fresh database required)
- Docker image name changed

## Testing
- All unit tests pass: ✅
- All integration tests pass: ✅
- Docker build successful: ✅
- API runs successfully: ✅

## Checklist
- [x] Build succeeds
- [x] All tests pass
- [x] Docker image builds
- [x] API runs correctly
- [x] Documentation updated
- [x] No hardcoded GenericAuth references remain
EOF
  )" \
  --base development \
  --head feature/rename-to-heimdallr

# 2. Or create PR manually on GitHub
echo "Create PR at: https://github.com/genuinecode-git/genericAuth/compare/development...feature/rename-to-heimdallr"
```

---

## 14. Post-Rename Verification Checklist

### 14.1 Build Verification

- [ ] Solution file loads without errors
- [ ] All 8 projects restore dependencies successfully
- [ ] All projects build in Debug configuration
- [ ] All projects build in Release configuration
- [ ] No compiler warnings related to namespaces
- [ ] Output assemblies have correct names (Heimdallr.*.dll)

### 14.2 Test Verification

- [ ] All Domain unit tests pass
- [ ] All Application unit tests pass
- [ ] All Infrastructure integration tests pass
- [ ] All API integration tests pass
- [ ] Test count matches pre-rename baseline
- [ ] No tests skipped or ignored
- [ ] Code coverage maintained (≥80%)

### 14.3 Runtime Verification

- [ ] API starts successfully with `dotnet run`
- [ ] Database migrations apply automatically
- [ ] Default admin user seeded
- [ ] Swagger UI accessible at root URL
- [ ] Health endpoint returns 200 OK
- [ ] Login endpoint works with default credentials
- [ ] JWT token generated with correct issuer/audience
- [ ] Refresh token flow works
- [ ] No errors in console logs

### 14.4 Configuration Verification

- [ ] appsettings.json has no "GenericAuth" references
- [ ] JWT Issuer is "Heimdallr"
- [ ] JWT Audience is "HeimdallrClient"
- [ ] Database connection string updated
- [ ] Logging namespace is "Heimdallr"
- [ ] launchSettings.json is correct

### 14.5 Docker Verification

- [ ] Dockerfile builds without errors
- [ ] Docker image tagged as heimdallr-api
- [ ] Container starts successfully
- [ ] Health check passes inside container
- [ ] API accessible from host
- [ ] No errors in container logs
- [ ] Container size reasonable (<200MB)

### 14.6 Docker Compose Verification

- [ ] docker-compose.yml syntax valid
- [ ] All services start successfully
- [ ] PostgreSQL database named "heimdallr"
- [ ] Redis container named "heimdallr-redis"
- [ ] Seq container named "heimdallr-seq"
- [ ] API container named "heimdallr-api"
- [ ] NGINX routes to API correctly
- [ ] Volumes named with "heimdallr" prefix
- [ ] Network named "heimdallr-network"

### 14.7 Kubernetes Verification

- [ ] YAML files are valid
- [ ] Namespace is "heimdallr"
- [ ] ConfigMap references updated
- [ ] Secrets references updated
- [ ] Deployment labels correct
- [ ] Service selectors match deployment
- [ ] HPA targets correct deployment
- [ ] NetworkPolicy references correct pods

### 14.8 CI/CD Verification

- [ ] .github/workflows/ci-cd.yml references Heimdallr.sln
- [ ] All test project paths updated
- [ ] .github/workflows/release.yml references Heimdallr.sln
- [ ] Docker image name is "heimdallr-api"
- [ ] Release notes template updated
- [ ] Workflows run successfully (or will on push)

### 14.9 Documentation Verification

- [ ] README.md updated throughout
- [ ] No "GenericAuth" in main heading
- [ ] badges updated (if project-specific)
- [ ] Architecture documentation updated
- [ ] API reference updated
- [ ] Getting started guide updated
- [ ] Postman collection renamed
- [ ] Postman collection variables updated
- [ ] Test documentation updated
- [ ] Implementation docs updated

### 14.10 Code Search Verification

```bash
# Should return ZERO results (except in comments/docs where contextual)
grep -r "GenericAuth" src/ tests/ \
  --include="*.cs" \
  --include="*.csproj" \
  --include="*.json" \
  | grep -v "admin@genericauth.com" \
  | grep -v "// Previously GenericAuth" \
  | grep -v "comment"

# Should return appropriate results
grep -r "Heimdallr" src/ tests/ --include="*.cs" | wc -l
# Expected: Similar count to old GenericAuth count
```

### 14.11 Git History Verification

- [ ] File renames tracked by git (git log --follow works)
- [ ] Commit messages are clear
- [ ] No accidental deletions
- [ ] All changes in feature branch
- [ ] Branch is up-to-date with development

### 14.12 Manual Smoke Testing

- [ ] Register new user via API
- [ ] Login as new user
- [ ] Create new application
- [ ] Create new role
- [ ] Assign role to user
- [ ] Test protected endpoint
- [ ] Logout
- [ ] Login as Auth Admin
- [ ] View all users
- [ ] Deactivate user
- [ ] Password reset flow
- [ ] Token refresh flow

---

## 15. Rollback Plan

### 15.1 Before Merge (Feature Branch)

**If problems found during testing**:
```bash
# Option 1: Abandon feature branch
git checkout development
git branch -D feature/rename-to-heimdallr

# Option 2: Fix issues and re-test
git checkout feature/rename-to-heimdallr
# Make fixes
git add .
git commit -m "fix: Address rename issues"
# Re-run all tests
```

### 15.2 After Merge (Development Branch)

**If critical issues found after merge to development**:
```bash
# Option 1: Revert merge commit
git checkout development
git revert <merge-commit-sha> -m 1
git push origin development

# Option 2: Cherry-pick fixes
git cherry-pick <fix-commit-sha>
git push origin development
```

### 15.3 After Deployment (Production)

**If issues found in production**:
```bash
# Option 1: Hotfix with revert
git checkout main
git revert <merge-commit-sha> -m 1
git push origin main
# Redeploy

# Option 2: Rollback deployment
# Use previous Docker image:
docker pull heimdallr-api:previous-version
# Or use GenericAuth image if still available
```

### 15.4 Database Rollback

**If database migration issues**:
```bash
# Option 1: Drop new database, restore old
dropdb heimdallr
pg_restore -d genericauth backup.dump

# Option 2: Rename database back
ALTER DATABASE heimdallr RENAME TO genericauth;

# Update connection string to old name
```

### 15.5 Point of No Return

**Once these actions occur, rollback is complex**:
1. Merge to `main` branch
2. Production deployment
3. Users create new accounts in renamed system
4. External integrations updated
5. DNS changes propagated
6. Old Docker images deleted

**Recommendation**: Extensive testing on `development` before merging to `main`.

---

## 16. Risk Assessment

### 16.1 High Risk Areas

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Namespace mismatch breaks build** | High | Low | Automated testing, careful regex |
| **JWT validation fails (issuer/audience)** | High | Medium | Document token invalidation |
| **Integration tests fail** | High | Low | Run tests before committing |
| **Docker image won't build** | High | Low | Test Docker build early |
| **Database migrations fail** | High | Low | Keep migrations unchanged |
| **CI/CD pipeline breaks** | Medium | Medium | Test workflow paths |
| **Production deployment fails** | High | Low | Test in staging environment |
| **Data loss from DB rename** | High | Medium | Backup before rename, document migration |

### 16.2 Medium Risk Areas

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Missed references in comments** | Low | High | Acceptable, fix iteratively |
| **Documentation inconsistencies** | Medium | Medium | Thorough documentation review |
| **Kubernetes deployment issues** | Medium | Medium | Validate YAML syntax |
| **NGINX config broken** | Medium | Low | Test load balancer config |
| **Test data references old names** | Low | Medium | Update test fixtures |

### 16.3 Low Risk Areas

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Git history confusion** | Low | Low | Use git mv, clear commit messages |
| **IDE cache issues** | Low | Medium | Clean and rebuild |
| **Old Docker volumes exist** | Low | High | Document volume cleanup |
| **README badges broken** | Low | Medium | Update badge URLs |

### 16.4 Risk Mitigation Strategy

1. **Comprehensive Testing**: Run full test suite at every phase
2. **Incremental Commits**: Commit after each logical phase
3. **Backup Everything**: Git tags, database exports, Docker images
4. **Test in Isolation**: Feature branch, no impact on development
5. **Peer Review**: PR review before merge
6. **Staging Deployment**: Test in staging before production
7. **Rollback Plan**: Document and test rollback procedures
8. **Communication**: Announce breaking changes (JWT tokens, database)

### 16.5 Acceptance Criteria

**Rename is successful if**:
- ✅ All builds succeed
- ✅ All tests pass (same count as before)
- ✅ API runs without errors
- ✅ Docker image builds and runs
- ✅ No "GenericAuth" in code (except migrations, comments)
- ✅ Documentation updated
- ✅ CI/CD workflows pass

**Rename is acceptable if**:
- ✅ Minor documentation inconsistencies (can fix iteratively)
- ✅ Old migrations still reference GenericAuth (expected)
- ✅ Comments mention "previously GenericAuth" (contextual)

**Rename should be rolled back if**:
- ❌ Build fails
- ❌ Tests fail
- ❌ API won't start
- ❌ Critical functionality broken
- ❌ Docker image won't build

---

## 17. Domain & Branding Considerations

### 17.1 Suggested Domain Names

**Primary Recommendations**:
1. **heimdallr.io** - Modern, tech-focused (.io is popular for developer tools)
2. **heimdallr.dev** - Developer-centric, aligns with auth/dev tools
3. **heimdallr.com** - Traditional, broader appeal

**Future Frontend Domains**:
- `auth.heimdallr.io` - User authentication portal
- `oauth.heimdallr.io` - OAuth service
- `admin.heimdallr.io` - Admin portal
- `api.heimdallr.io` - API gateway
- `docs.heimdallr.io` - Documentation site

### 17.2 Logo & Branding

**Heimdallr Themes**:
- **Norse mythology**: Guardian, watchful, bridge-keeper
- **Colors**: Gold/amber (Heimdallr's golden teeth), blue (Bifrost bridge)
- **Symbols**: Rainbow bridge, horn (Gjallarhorn), sword
- **Concepts**: Security, vigilance, gateway, access control

**Logo Ideas**:
- Stylized Bifrost bridge
- Nordic runes forming "H"
- Horn silhouette
- Rainbow gradient with shield
- Minimalist guardian figure

### 17.3 Marketing Materials

**Tagline Options**:
- "Guardian of Your Authentication"
- "Secure the Bifrost to Your Applications"
- "Watchful Authentication for Modern Apps"
- "The Bridge to Secure Access"

**Elevator Pitch**:
"Heimdallr is a production-ready, multi-tenant authentication and authorization API built with Clean Architecture. Named after the Norse guardian of the gods, Heimdallr watches over your application access with enterprise-grade security, JWT authentication, and flexible role-based permissions."

### 17.4 Social Media

**Handles to Secure**:
- GitHub: `heimdallr-auth` or `heimdallr-io`
- Twitter/X: `@heimdallr_auth` or `@heimdallrauth`
- LinkedIn: Heimdallr Authentication Platform
- Docker Hub: `heimdallr` or `heimdallr-auth`
- NPM (future): `@heimdallr/*` scope

### 17.5 Repository Rename

**GitHub Repository**:
```
Current: github.com/genuinecode-git/genericAuth
Proposed: github.com/genuinecode-git/heimdallr

# Or new organization:
github.com/heimdallr-auth/heimdallr
```

**Note**: GitHub automatically creates redirects from old repository names.

### 17.6 Package Names (Future)

**NuGet Packages** (if published):
- `Heimdallr.Core`
- `Heimdallr.Client`
- `Heimdallr.AspNetCore`

**NPM Packages** (future frontend):
- `@heimdallr/sdk`
- `@heimdallr/react`
- `@heimdallr/components`

### 17.7 Trademark Considerations

**Research**:
- Check USPTO database for "Heimdallr" in software/auth category
- Verify domain availability (.io, .dev, .com)
- Search GitHub for similar projects
- Google "Heimdallr authentication" for conflicts

**Known Projects** (as of 2025):
- Heimdall (Docker dashboard) - different space, no conflict
- Various Norse mythology references - generally acceptable

---

## 18. Estimated Time & Complexity

### 18.1 Time Breakdown

| Phase | Task | Estimated Time |
|-------|------|---------------|
| 1 | Preparation & Backup | 15 minutes |
| 2 | Folder & File Renames | 30 minutes |
| 3 | Solution File Update | 10 minutes |
| 4 | Project File Updates | 15 minutes |
| 5 | Source Code Namespaces | 45 minutes |
| 6 | Configuration Files | 30 minutes |
| 7 | Documentation Updates | 30 minutes |
| 8 | Build & Test Verification | 30 minutes |
| 9 | Runtime Testing | 15 minutes |
| 10 | Docker Testing | 15 minutes |
| 11 | Final Review & Commit | 15 minutes |
| 12 | Create Pull Request | 10 minutes |
| **Total** | **Execution** | **4 hours** |

**Additional Time**:
- PR Review & Feedback: 1-2 hours
- Addressing Issues: 0-2 hours
- Final Testing & Merge: 30 minutes

**Total Project Time**: **6-8 hours** (including review cycle)

### 18.2 Complexity Rating

**Overall Complexity**: **7/10** (High)

**Breakdown**:
- **Technical Complexity**: 6/10
  - Straightforward file/namespace renames
  - Well-defined structure
  - Good tooling support (git mv, IDE refactoring)

- **Risk Complexity**: 8/10
  - Many files to change (~100+)
  - Breaking changes (JWT, database)
  - Integration test dependencies

- **Testing Complexity**: 7/10
  - Comprehensive test suite required
  - Docker/Kubernetes validation
  - CI/CD verification

### 18.3 Skill Level Required

**Recommended Experience**:
- ✅ Strong understanding of .NET project structure
- ✅ Familiarity with Clean Architecture
- ✅ Git proficiency (branching, merging, history)
- ✅ Docker and containerization knowledge
- ✅ Regex and find/replace expertise
- ✅ Testing and debugging skills

**Not Required**:
- Advanced C# language features
- Deep Clean Architecture expertise
- DevOps specialization

### 18.4 Team Size

**Ideal**: 1-2 developers

**Roles**:
- **Primary**: Executes rename, testing, documentation
- **Reviewer**: Code review, PR approval, testing validation

**Solo**: Feasible with careful execution and self-review

### 18.5 When to Perform

**Best Time**:
- ✅ Early in project lifecycle (less historical debt)
- ✅ Between major releases
- ✅ Low activity period (fewer concurrent PRs)
- ✅ After major features are stable
- ✅ Before production launch (if possible)

**Avoid**:
- ❌ During active feature development
- ❌ Right before release deadline
- ❌ When multiple developers have open PRs
- ❌ During production issues
- ❌ When team is unavailable for review

### 18.6 Effort vs. Benefit

**Effort**: **HIGH** (6-8 hours, potential for issues)

**Benefit**: **HIGH**
- ✅ Professional, memorable brand (Heimdallr)
- ✅ Better marketing and positioning
- ✅ Unique identity (not "generic")
- ✅ Aligns with mythology (guardian theme)
- ✅ Domain availability likely
- ✅ Clean slate for future branding

**Recommendation**: **Proceed with rename**

The effort is justified given:
1. **Early Stage**: Project not yet in production
2. **Clean Codebase**: Well-structured, minimal technical debt
3. **Brand Value**: "Heimdallr" is significantly better than "GenericAuth"
4. **Future-Proofing**: Easier now than after user adoption

---

## Summary & Next Steps

### TL;DR

**What**: Rename GenericAuth to Heimdallr across entire codebase
**Why**: Better branding, professional identity, memorable name
**When**: Now (before production deployment)
**How**: Systematic rename using git mv + find/replace
**Time**: 4-6 hours execution, 6-8 hours total
**Risk**: Medium-High (many files, breaking changes)
**Recommendation**: Proceed with feature branch + thorough testing

### Immediate Next Steps

1. ✅ Review this plan thoroughly
2. ✅ Get team consensus on name change
3. ✅ Check domain availability (heimdallr.io, .dev, .com)
4. ✅ Schedule rename during low-activity period
5. ✅ Create backup (git tag)
6. ✅ Execute Phase 1-12 systematically
7. ✅ Create PR for review
8. ✅ Merge after approval
9. ✅ Update external references (if any)
10. ✅ Announce rename in release notes

### Long-Term Follow-Up

1. ✅ Acquire domain names
2. ✅ Design logo and branding
3. ✅ Update marketing materials
4. ✅ Rename GitHub repository
5. ✅ Update README badges
6. ✅ Announce on social media
7. ✅ Update any external integrations
8. ✅ Consider trademark registration

---

## Appendix: Useful Commands

### A. Find All References

```bash
# Find all "GenericAuth" in code
grep -r "GenericAuth" src/ tests/ --include="*.cs"

# Find all "GenericAuth" in config
grep -r "GenericAuth" . --include="*.json" --include="*.yml" --include="*.yaml"

# Find all "genericauth" (lowercase)
grep -r "genericauth" . -i
```

### B. Bulk Replace

```bash
# macOS/Linux: Replace in all .cs files
find . -name "*.cs" -exec sed -i '' 's/GenericAuth/Heimdallr/g' {} +

# macOS/Linux: Replace in all config files
find . \( -name "*.json" -o -name "*.yml" -o -name "*.yaml" \) -exec sed -i '' 's/GenericAuth/Heimdallr/g' {} +
```

### C. Verify Changes

```bash
# Count changes
git diff --name-only HEAD | wc -l

# Show changed files
git diff --name-only HEAD

# Show full diff
git diff HEAD
```

### D. Test Commands

```bash
# Clean build
dotnet clean && dotnet build

# Run all tests with summary
dotnet test --configuration Release --verbosity normal

# Run specific test project
dotnet test tests/Heimdallr.API.IntegrationTests/
```

### E. Docker Commands

```bash
# Build image
docker build -t heimdallr-api:latest .

# Run container
docker run -d -p 8080:8080 --name heimdallr heimdallr-api:latest

# Check logs
docker logs heimdallr

# Cleanup
docker stop heimdallr && docker rm heimdallr
```

---

**End of Rename Plan**

For questions or issues during execution, refer to specific sections above. Good luck with the rename!
