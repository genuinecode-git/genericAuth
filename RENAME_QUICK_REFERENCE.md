# Heimdallr Rename - Quick Reference Guide

**Quick access commands and checklists for the GenericAuth → Heimdallr rename.**

For full details, see [RENAME_TO_HEIMDALLR_PLAN.md](./RENAME_TO_HEIMDALLR_PLAN.md)

---

## Quick Start

```bash
# 1. Backup & branch
git tag pre-heimdallr-rename-$(date +%Y%m%d)
git checkout -b feature/rename-to-heimdallr

# 2. Run automated rename script (see below)
./rename-to-heimdallr.sh

# 3. Test
dotnet test Heimdallr.sln --configuration Release

# 4. Commit & PR
git add .
git commit -m "refactor: Rename project from GenericAuth to Heimdallr"
git push origin feature/rename-to-heimdallr
```

---

## Automated Rename Script

Create `rename-to-heimdallr.sh` in project root:

```bash
#!/bin/bash
set -e  # Exit on error

echo "🚀 Starting GenericAuth → Heimdallr rename..."

# Phase 1: Rename folders and files (use git mv)
echo "📁 Phase 1: Renaming folders and files..."

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

# Tests
git mv tests/GenericAuth.Domain.UnitTests tests/Heimdallr.Domain.UnitTests
git mv tests/Heimdallr.Domain.UnitTests/GenericAuth.Domain.UnitTests.csproj tests/Heimdallr.Domain.UnitTests/Heimdallr.Domain.UnitTests.csproj

git mv tests/GenericAuth.Application.UnitTests tests/Heimdallr.Application.UnitTests
git mv tests/Heimdallr.Application.UnitTests/GenericAuth.Application.UnitTests.csproj tests/Heimdallr.Application.UnitTests/Heimdallr.Application.UnitTests.csproj

git mv tests/GenericAuth.Infrastructure.IntegrationTests tests/Heimdallr.Infrastructure.IntegrationTests
git mv tests/Heimdallr.Infrastructure.IntegrationTests/GenericAuth.Infrastructure.IntegrationTests.csproj tests/Heimdallr.Infrastructure.IntegrationTests/Heimdallr.Infrastructure.IntegrationTests.csproj

git mv tests/GenericAuth.API.IntegrationTests tests/Heimdallr.API.IntegrationTests
git mv tests/Heimdallr.API.IntegrationTests/GenericAuth.API.IntegrationTests.csproj tests/Heimdallr.API.IntegrationTests/Heimdallr.API.IntegrationTests.csproj

# Solution
git mv GenericAuth.sln Heimdallr.sln

# Docs
git mv docs/GenericAuth.postman_collection.json docs/Heimdallr.postman_collection.json

git commit -m "refactor: Rename folders and files from GenericAuth to Heimdallr"

# Phase 2: Update file contents
echo "📝 Phase 2: Updating file contents..."

# Solution file
sed -i '' 's/GenericAuth\./Heimdallr./g' Heimdallr.sln

# Project files
find src tests -name "*.csproj" -type f -exec sed -i '' 's/<RootNamespace>GenericAuth\./<RootNamespace>Heimdallr./g' {} +
find src tests -name "*.csproj" -type f -exec sed -i '' 's/<AssemblyName>GenericAuth\./<AssemblyName>Heimdallr./g' {} +
find src tests -name "*.csproj" -type f -exec sed -i '' 's/GenericAuth\./Heimdallr./g' {} +

# C# files
find src tests -name "*.cs" -type f -exec sed -i '' 's/namespace GenericAuth\./namespace Heimdallr./g' {} +
find src tests -name "*.cs" -type f -exec sed -i '' 's/using GenericAuth\./using Heimdallr./g' {} +

# Configuration files
sed -i '' 's/"GenericAuth":/"Heimdallr":/g' src/Heimdallr.API/appsettings.json
sed -i '' 's/GenericAuth\.db/Heimdallr.db/g' src/Heimdallr.API/appsettings.json
sed -i '' 's/"Issuer": "GenericAuth"/"Issuer": "Heimdallr"/g' src/Heimdallr.API/appsettings.json
sed -i '' 's/"Audience": "GenericAuthClient"/"Audience": "HeimdallrClient"/g' src/Heimdallr.API/appsettings.json

# Docker files
sed -i '' 's/GenericAuth/Heimdallr/g' Dockerfile
sed -i '' 's/genericauth/heimdallr/g' docker-compose.yml
sed -i '' 's/GenericAuth/Heimdallr/g' docker-compose.yml

if [ -f docker-stack.yml ]; then
  sed -i '' 's/genericauth/heimdallr/g' docker-stack.yml
  sed -i '' 's/GenericAuth/Heimdallr/g' docker-stack.yml
fi

# .env.template
sed -i '' 's/genericauth/heimdallr/g' .env.template
sed -i '' 's/GenericAuth/Heimdallr/g' .env.template

# Kubernetes
sed -i '' 's/genericauth/heimdallr/g' kubernetes/deployment.yaml
sed -i '' 's/GenericAuth/Heimdallr/g' kubernetes/deployment.yaml
sed -i '' 's/genericauth/heimdallr/g' kubernetes/ingress.yaml
sed -i '' 's/GenericAuth/Heimdallr/g' kubernetes/ingress.yaml

# GitHub Actions
sed -i '' "s/SOLUTION_PATH: 'GenericAuth\.sln'/SOLUTION_PATH: 'Heimdallr.sln'/g" .github/workflows/ci-cd.yml
sed -i '' 's/GenericAuth\./Heimdallr./g' .github/workflows/ci-cd.yml
sed -i '' "s/SOLUTION_PATH: 'GenericAuth\.sln'/SOLUTION_PATH: 'Heimdallr.sln'/g" .github/workflows/release.yml
sed -i '' "s/DOCKER_IMAGE_NAME: 'genericauth-api'/DOCKER_IMAGE_NAME: 'heimdallr-api'/g" .github/workflows/release.yml
sed -i '' 's/GenericAuth/Heimdallr/g' .github/workflows/release.yml
sed -i '' 's/genericauth/heimdallr/g' .github/workflows/release.yml

# Documentation
find . -name "*.md" -not -path "./.git/*" -not -path "./RENAME_*.md" -type f -exec sed -i '' 's/GenericAuth/Heimdallr/g' {} +
find . -name "*.md" -not -path "./.git/*" -not -path "./RENAME_*.md" -type f -exec sed -i '' 's/genericAuth/heimdallr/g' {} +
find . -name "*.md" -not -path "./.git/*" -not -path "./RENAME_*.md" -type f -exec sed -i '' 's/genericauth/heimdallr/g' {} +

# Postman collection
sed -i '' 's/GenericAuth/Heimdallr/g' docs/Heimdallr.postman_collection.json

git add .
git commit -m "refactor: Update file contents for Heimdallr rename"

# Phase 3: Build and test
echo "🔨 Phase 3: Building and testing..."
dotnet clean Heimdallr.sln
dotnet restore Heimdallr.sln
dotnet build Heimdallr.sln --configuration Release

echo "🧪 Running tests..."
dotnet test Heimdallr.sln --configuration Release --verbosity normal

echo ""
echo "✅ Rename complete!"
echo ""
echo "Next steps:"
echo "1. Review changes: git diff pre-heimdallr-rename-$(date +%Y%m%d)..HEAD"
echo "2. Manual verification (see checklist below)"
echo "3. Push branch: git push origin feature/rename-to-heimdallr"
echo "4. Create PR on GitHub"
```

Make executable:
```bash
chmod +x rename-to-heimdallr.sh
```

---

## Manual Verification Checklist

After running the script, verify:

### Build & Tests
- [ ] `dotnet build Heimdallr.sln` succeeds
- [ ] All tests pass: `dotnet test Heimdallr.sln`
- [ ] Test count matches baseline

### Runtime
```bash
cd src/Heimdallr.API
dotnet run
```
- [ ] API starts without errors
- [ ] Swagger UI loads: `http://localhost:5071`
- [ ] Health endpoint: `curl http://localhost:5071/health`
- [ ] Login works with admin@genericauth.com / Admin@123

### Docker
```bash
docker build -t heimdallr-api:test .
docker run -d -p 8080:8080 --name test heimdallr-api:test
curl http://localhost:8080/health
docker stop test && docker rm test
```
- [ ] Docker builds successfully
- [ ] Container runs
- [ ] Health check passes

### No Remaining References
```bash
# Should return ZERO (except migrations, admin email, comments)
grep -r "GenericAuth" src/ tests/ \
  --include="*.cs" \
  --include="*.csproj" \
  | grep -v "admin@genericauth.com" \
  | grep -v "namespace GenericAuth.Infrastructure.Migrations" \
  | grep -v "//"
```

---

## Key Changes Summary

### Breaking Changes
1. **JWT Tokens**: Issuer/Audience changed → All tokens invalid
2. **Database Name**: `genericauth` → `heimdallr` (new DB)
3. **Docker Image**: `genericauth-api` → `heimdallr-api`

### Configuration Changes
- **appsettings.json**:
  - Issuer: `GenericAuth` → `Heimdallr`
  - Audience: `GenericAuthClient` → `HeimdallrClient`
  - LogLevel: `GenericAuth` → `Heimdallr`
  - Connection: `GenericAuth.db` → `Heimdallr.db`

- **docker-compose.yml**:
  - All service names: `genericauth-*` → `heimdallr-*`
  - Database name: `genericauth` → `heimdallr`
  - Network: `genericauth-network` → `heimdallr-network`
  - Volumes: `genericauth-*-data` → `heimdallr-*-data`

- **Kubernetes**:
  - Namespace: `genericauth` → `heimdallr`
  - All resources prefixed with `heimdallr-*`

---

## Troubleshooting

### Build Fails
```bash
# Clean everything
dotnet clean
rm -rf **/bin **/obj
dotnet restore
dotnet build
```

### Test Fails
```bash
# Delete databases
find . -name "*.db*" -delete

# Rebuild and test
dotnet build --configuration Release
dotnet test --configuration Release
```

### Namespace Errors
```bash
# Check for missed namespaces
grep -r "namespace GenericAuth" src/ tests/

# Check for missed using statements
grep -r "using GenericAuth" src/ tests/
```

### Docker Build Fails
```bash
# Check Dockerfile paths
grep "GenericAuth" Dockerfile  # Should be empty

# Rebuild
docker build -t heimdallr-api:test .
```

---

## Rollback Commands

If something goes wrong:

```bash
# Before committing
git reset --hard HEAD

# After committing (undo last commit)
git reset --soft HEAD~1

# After pushing (revert)
git revert HEAD
git push origin feature/rename-to-heimdallr

# Abandon feature branch
git checkout development
git branch -D feature/rename-to-heimdallr
```

---

## Post-Merge Tasks

After PR is merged to `development`:

1. **Update local repository**:
```bash
git checkout development
git pull origin development
git branch -d feature/rename-to-heimdallr
```

2. **Test on development**:
```bash
dotnet test Heimdallr.sln
cd src/Heimdallr.API && dotnet run
```

3. **Docker Compose test**:
```bash
docker-compose up --build
```

4. **Document breaking changes in CHANGELOG.md**

5. **Update VERSION file for next release** (if major version bump)

---

## PR Template Checklist

When creating PR, ensure:

- [ ] All project files renamed
- [ ] All namespaces updated
- [ ] All config files updated
- [ ] All documentation updated
- [ ] Build succeeds
- [ ] All tests pass (same count)
- [ ] Docker builds successfully
- [ ] API runs and responds
- [ ] Breaking changes documented
- [ ] Migration guide provided

---

## Quick Reference: File Counts

Before rename:
- Solution files: 1 (GenericAuth.sln)
- Project files: 8 (*.csproj)
- C# namespace declarations: ~100+
- C# using statements: ~200+
- Config files: 10+
- Documentation files: 15+

After rename (should match):
- Solution files: 1 (Heimdallr.sln)
- Project files: 8 (*.csproj)
- Heimdallr namespaces: ~100+
- Heimdallr using statements: ~200+

---

## Contact & Support

For issues during rename:
1. Check full plan: [RENAME_TO_HEIMDALLR_PLAN.md](./RENAME_TO_HEIMDALLR_PLAN.md)
2. Search for error messages in plan (Section 11: Testing Strategy)
3. Review troubleshooting section above
4. Check git history: `git log --oneline --graph`

---

**Good luck with the rename! 🚀**
