# Release Workflow Documentation

## Overview

The Release workflow (`release.yml`) is a GitHub Actions workflow that automates the build, test, and release process for the GenericAuth API, including Docker image packaging.

## Features

- ✅ **Manual Trigger**: Runs on-demand via GitHub Actions UI
- ✅ **Automatic Version Increment**: Intelligently increments version numbers
- ✅ **Build & Test**: Compiles the .NET solution and runs all tests
- ✅ **Docker Build**: Creates production-ready Docker images
- ✅ **Release Packaging**: Saves Docker images as compressed tar.gz files
- ✅ **GitHub Release**: Automatically creates a GitHub release with artifacts
- ✅ **Checksums**: Generates SHA256 checksums for verification

## How to Use

### Trigger a Release

1. Go to your GitHub repository
2. Click on **Actions** tab
3. Select **Release Build & Docker Package** workflow
4. Click **Run workflow** button
5. Choose version increment type:
   - **patch** (default): Increments the last digit (1.0.0.0 → 1.0.0.1)
   - **minor**: Increments minor version (1.0.0.0 → 1.1.0.0)
   - **major**: Increments major version (1.0.0.0 → 2.0.0.0)
6. Click **Run workflow**

### Version Numbering

The workflow uses a 4-part semantic versioning scheme: `MAJOR.MINOR.PATCH.BUILD`

- **MAJOR**: Breaking changes or major features
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes and minor improvements
- **BUILD**: Automatic increment for each release (default)

Current version is stored in the `VERSION` file at the repository root.

## Workflow Steps

### 1. Version Management
- Reads current version from `VERSION` file
- Increments based on selected type
- Commits updated version back to repository

### 2. Build & Test
- Restores NuGet dependencies
- Builds the entire solution in Release mode
- Runs all unit and integration tests
- Fails if any test fails

### 3. Docker Image Build
Creates three tagged images:
- `genericauth-api:<version>` (e.g., `genericauth-api:1.0.0.1`)
- `genericauth-api:latest`
- `genericauth-api:v<semver>` (e.g., `genericauth-api:v1.0.0`)

### 4. Package & Release
- Saves Docker images as tar archives
- Compresses with gzip for efficient storage
- Generates SHA256 checksums
- Creates GitHub Release with all artifacts
- Uploads artifacts for 90-day retention

## Release Artifacts

Each release includes:

### Docker Images (compressed)
- `genericauth-api-<version>.tar.gz` - Versioned image
- `genericauth-api-latest.tar.gz` - Latest image

### Documentation
- `RELEASE_NOTES.md` - Release information and usage instructions
- `checksums.txt` - SHA256 checksums for verification

## Using Released Docker Images

### Download and Load Image

```bash
# Download the release from GitHub Releases page
wget https://github.com/<your-org>/<your-repo>/releases/download/v1.0.0.1/genericauth-api-1.0.0.1.tar.gz

# Verify checksum
wget https://github.com/<your-org>/<your-repo>/releases/download/v1.0.0.1/checksums.txt
sha256sum -c checksums.txt

# Extract and load into Docker
gunzip -c genericauth-api-1.0.0.1.tar.gz | docker load

# Verify image is loaded
docker images | grep genericauth-api
```

### Run the Container

```bash
# Run with default settings
docker run -d \
  -p 8080:8080 \
  -p 8081:8081 \
  --name genericauth-api \
  genericauth-api:1.0.0.1

# Run with environment variables
docker run -d \
  -p 8080:8080 \
  -p 8081:8081 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  --name genericauth-api \
  genericauth-api:1.0.0.1

# Check logs
docker logs genericauth-api

# Test health endpoint
curl http://localhost:8080/health
```

### Using Docker Compose

```yaml
version: '3.8'

services:
  api:
    image: genericauth-api:1.0.0.1
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=your-connection-string
    restart: unless-stopped
```

## Troubleshooting

### Workflow Fails at Tests
- Check test results in the workflow logs
- Fix failing tests before creating release
- Tests must pass for release to proceed

### Docker Build Fails
- Verify Dockerfile is present and valid
- Check that all required files are committed
- Ensure .dockerignore is not excluding needed files

### Version Conflict
- If version update fails, manually update `VERSION` file
- Ensure you have write permissions to the repository
- Check that branch protection rules allow workflow commits

### Release Already Exists
- Delete the existing release and tag if needed
- Or increment to a higher version number
- Releases cannot overwrite existing tags

## Best Practices

1. **Before Release**
   - Ensure all tests pass locally
   - Update documentation if needed
   - Review changes since last release

2. **Version Selection**
   - Use **patch** for regular releases and bug fixes
   - Use **minor** for new features
   - Use **major** for breaking changes

3. **After Release**
   - Verify release artifacts are available
   - Test the Docker image from the release
   - Update deployment documentation if needed

4. **Security**
   - Released images are production builds
   - Verify checksums before deploying
   - Keep released versions for rollback capability

## Automation

The workflow is designed to be triggered manually, but can be extended to:
- Auto-trigger on tags: `on: push: tags: - 'v*'`
- Schedule periodic releases: `on: schedule: - cron: '0 0 * * 0'`
- Trigger from other workflows: `workflow_call`

## Permissions Required

The workflow requires:
- `contents: write` - To commit version updates and create releases
- `packages: write` - For future GitHub Container Registry support

## Environment Variables

Key environment variables in the workflow:
- `DOTNET_VERSION`: .NET SDK version (currently 9.0.x)
- `SOLUTION_PATH`: Path to .sln file
- `DOCKER_IMAGE_NAME`: Base name for Docker images
- `REGISTRY`: Container registry (prepared for ghcr.io)

## Future Enhancements

Planned improvements:
- [ ] Push images to GitHub Container Registry (ghcr.io)
- [ ] Multi-architecture builds (linux/amd64, linux/arm64)
- [ ] Automated changelog generation
- [ ] Deployment notifications (Slack, email)
- [ ] Smoke tests on built Docker image
- [ ] Security scanning with Trivy
