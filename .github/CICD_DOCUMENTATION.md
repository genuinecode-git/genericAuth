# CI/CD Pipeline Documentation

## Overview

This project includes a comprehensive CI/CD pipeline using GitHub Actions that ensures code quality through automated builds, tests, and code coverage analysis.

## Pipeline Configuration

**File**: `.github/workflows/ci-cd.yml`

### Triggers

The pipeline runs on:
- **Push to `main` branch**: Every commit to main triggers the pipeline
- **Pull Requests to `main` branch**: All PRs are validated before merge

### Pipeline Stages

```
┌─────────────────────────────────────────────────────────┐
│  1. Checkout Code                                       │
├─────────────────────────────────────────────────────────┤
│  2. Setup .NET 8.0                                      │
├─────────────────────────────────────────────────────────┤
│  3. Cache NuGet Packages                                │
├─────────────────────────────────────────────────────────┤
│  4. Restore Dependencies                                │
├─────────────────────────────────────────────────────────┤
│  5. Build Solution (Release)                            │
├─────────────────────────────────────────────────────────┤
│  6. Run Tests with Coverage                             │
│     ├── Domain Unit Tests                               │
│     ├── Application Unit Tests                          │
│     ├── Infrastructure Integration Tests                │
│     └── API Integration Tests                           │
├─────────────────────────────────────────────────────────┤
│  7. Generate Coverage Report                            │
├─────────────────────────────────────────────────────────┤
│  8. Enforce Coverage Threshold (80%)                    │
├─────────────────────────────────────────────────────────┤
│  9. Upload Artifacts                                    │
│     ├── Test Results (.trx files)                       │
│     └── Coverage Report (HTML + JSON)                   │
└─────────────────────────────────────────────────────────┘
```

## Code Coverage

### Coverage Tools

- **Coverlet**: Cross-platform code coverage library for .NET
- **ReportGenerator**: Generates human-readable coverage reports

### Coverage Threshold

**Minimum Required**: 80% line coverage

The pipeline will **FAIL** if code coverage falls below 80%.

### Coverage Reports

The pipeline generates multiple coverage report formats:

1. **HTML Report**: Interactive web-based report
2. **JSON Summary**: Parseable summary for automation
3. **Badges**: SVG badges for README
4. **Cobertura**: XML format for third-party tools

### Viewing Coverage Reports

Coverage reports are uploaded as artifacts and can be downloaded from:
- GitHub Actions run summary
- Artifacts section of each workflow run

### Coverage Report Structure

```
coverage/report/
├── index.html              # Main coverage report (open in browser)
├── Summary.json            # JSON summary with metrics
├── Cobertura.xml          # Cobertura format for integrations
└── badge_linecoverage.svg  # Coverage badge
```

## Test Execution

### Test Projects

The pipeline runs tests from all test projects:

1. **GenericAuth.Domain.UnitTests**
   - Tests domain entities, value objects, and business logic
   - Should have highest coverage (90%+)

2. **GenericAuth.Application.UnitTests**
   - Tests CQRS handlers, validators, behaviors
   - Target coverage: 85%+

3. **GenericAuth.Infrastructure.IntegrationTests**
   - Tests database operations, repositories, EF Core configurations
   - Target coverage: 75%+

4. **GenericAuth.API.IntegrationTests**
   - Tests API endpoints, controllers, middleware
   - Target coverage: 70%+

### Test Results

Test results are generated in TRX format and uploaded as artifacts:
- `domain-test-results.trx`
- `application-test-results.trx`
- `infrastructure-test-results.trx`
- `api-test-results.trx`

## Environment Configuration

### Environment Variables

```yaml
DOTNET_VERSION: '8.0.x'         # .NET SDK version
SOLUTION_PATH: 'GenericAuth.sln' # Solution file path
COVERAGE_THRESHOLD: 80           # Minimum coverage percentage
```

### Required Secrets (Optional)

- `CODECOV_TOKEN`: For Codecov integration (optional)

## Build Optimization

### Caching Strategy

The pipeline caches NuGet packages to speed up builds:

```yaml
Cache Key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
```

This reduces build time by:
- Avoiding re-downloading packages
- Reusing restored dependencies

**Typical Build Times**:
- First build (no cache): ~2-3 minutes
- Subsequent builds (cached): ~30-60 seconds

## Artifacts

### Available Artifacts

After each run, the following artifacts are available:

#### 1. Test Results
- **Name**: `test-results`
- **Contents**: All .trx test result files
- **Retention**: 30 days

#### 2. Coverage Report
- **Name**: `coverage-report`
- **Contents**: HTML report, JSON summary, badges
- **Retention**: 30 days

### Downloading Artifacts

1. Navigate to Actions tab in GitHub
2. Click on a workflow run
3. Scroll to "Artifacts" section
4. Download desired artifact

## Coverage Threshold Enforcement

### How It Works

The pipeline extracts line coverage percentage from `Summary.json`:

```bash
COVERAGE=$(cat ./coverage/report/Summary.json | grep -o '"linecoverage":"[^"]*' | cut -d'"' -f4)
```

Then compares it against the threshold:

```bash
if [ "$COVERAGE_INT" -lt "80" ]; then
  echo "❌ Coverage check FAILED"
  exit 1
else
  echo "✅ Coverage check PASSED"
fi
```

### Failure Scenarios

The pipeline will fail if:
1. **Build fails**: Compilation errors
2. **Tests fail**: Any test failure
3. **Coverage < 80%**: Line coverage below threshold
4. **Coverage report missing**: Report generation fails

## Pull Request Integration

### PR Comments

For pull requests, the pipeline:
1. Posts coverage summary as a PR comment
2. Updates the comment on subsequent pushes
3. Shows coverage delta compared to base branch

### PR Status Checks

GitHub shows the pipeline status on PRs:
- ✅ **Green**: All checks passed, coverage ≥ 80%
- ❌ **Red**: Build failed, tests failed, or coverage < 80%

## Codecov Integration (Optional)

The pipeline includes optional Codecov integration:

```yaml
- name: Upload Coverage to Codecov
  uses: codecov/codecov-action@v4
  with:
    files: ./coverage/report/Cobertura.xml
    token: ${{ secrets.CODECOV_TOKEN }}
```

### Setting Up Codecov

1. Sign up at [codecov.io](https://codecov.io)
2. Add repository to Codecov
3. Copy the repository token
4. Add as `CODECOV_TOKEN` secret in GitHub repository settings

## Troubleshooting

### Common Issues

#### 1. Coverage Below 80%

**Problem**: Pipeline fails with "Coverage check FAILED"

**Solution**:
- Write more unit tests for uncovered code
- Check coverage report to identify gaps
- Focus on Domain and Application layers (highest value)

#### 2. Test Failures

**Problem**: One or more tests fail

**Solution**:
- Review test logs in GitHub Actions
- Download test results artifacts
- Run tests locally: `dotnet test`

#### 3. Build Failures

**Problem**: Compilation errors

**Solution**:
- Ensure code builds locally
- Check for missing dependencies
- Review build logs in Actions tab

#### 4. Coverage Report Not Generated

**Problem**: "Coverage report not found" error

**Solution**:
- Ensure Coverlet package is installed
- Verify test execution completed
- Check for errors in test output

### Debugging Locally

To run coverage locally:

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:"Html"

# Open report
open ./coverage/report/index.html
```

## Performance Metrics

### Expected Run Times

| Stage | Duration |
|-------|----------|
| Checkout & Setup | 10-15 seconds |
| Restore Dependencies | 30-60 seconds (cached) |
| Build Solution | 20-30 seconds |
| Run All Tests | 30-90 seconds |
| Generate Coverage | 10-15 seconds |
| Upload Artifacts | 10-20 seconds |
| **Total** | **2-4 minutes** |

## Best Practices

### Writing Testable Code

1. **Use dependency injection**: Makes mocking easier
2. **Small, focused methods**: Easier to test thoroughly
3. **Avoid static methods**: Difficult to mock
4. **Pure functions**: No side effects, easy to test

### Test Coverage Guidelines

- **Domain Layer**: Aim for 90%+ (core business logic)
- **Application Layer**: Aim for 85%+ (handlers, validators)
- **Infrastructure Layer**: Aim for 75%+ (data access)
- **API Layer**: Aim for 70%+ (controllers, middleware)

### Maintaining Coverage

1. **Write tests first** (TDD approach)
2. **Test before pushing** to catch issues early
3. **Review coverage reports** regularly
4. **Don't ignore low coverage** warnings

## Continuous Improvement

### Monitoring Trends

Track these metrics over time:
- Overall coverage percentage
- Per-project coverage
- Number of tests
- Build duration

### Recommended Actions

- **Monthly**: Review coverage trends
- **Quarterly**: Refactor to improve testability
- **Annually**: Evaluate coverage targets

## Additional Resources

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [xUnit Documentation](https://xunit.net/)

## Support

For issues with the CI/CD pipeline:
1. Check this documentation
2. Review GitHub Actions logs
3. Open an issue in the repository
