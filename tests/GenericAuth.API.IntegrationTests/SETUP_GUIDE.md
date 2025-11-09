# Integration Tests Setup Guide

## Quick Fix for Database Provider Conflict

The integration tests are experiencing a conflict between SQLite (production) and InMemory (testing) database providers. Here are the recommended solutions:

### Solution 1: Use SQLite In-Memory (Recommended)

Modify `CustomWebApplicationFactory.cs`:

```csharp
// Add in-memory SQLite database for testing
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("DataSource=:memory:");
    options.EnableSensitiveDataLogging();
});

// Ensure the connection stays open
var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
context.Database.OpenConnection();
context.Database.EnsureCreated();
```

### Solution 2: Modify Infrastructure Layer

Add environment-based database provider selection in `Infrastructure/DependencyInjection.cs`:

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment)
{
    if (environment.IsEnvironment("Testing"))
    {
        // Use in-memory database for testing
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("InMemoryDbForTesting"));
    }
    else
    {
        // Use SQLite for production
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
    }

    // Rest of the registrations...
}
```

Then update `Program.cs`:

```csharp
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
```

### Solution 3: Test Containers (Production-Grade)

Use Testcontainers for real database testing:

1. Add package:
```bash
dotnet add package Testcontainers.MsSql
```

2. Update `CustomWebApplicationFactory.cs`:
```csharp
private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    _dbContainer.StartAsync().Wait();

    builder.ConfigureTestServices(services =>
    {
        // Remove existing registrations...

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(_dbContainer.GetConnectionString());
        });
    });
}

public override async ValueTask DisposeAsync()
{
    await _dbContainer.DisposeAsync();
    await base.DisposeAsync();
}
```

## Running the Tests After Fix

```bash
cd tests/GenericAuth.API.IntegrationTests
dotnet test
```

Expected output:
```
Test Run Successful.
Total tests: 78
     Passed: 78
```

## Verification Checklist

After applying the fix:

- [ ] All 78 tests compile without errors
- [ ] Database provider conflict is resolved
- [ ] Tests can run independently
- [ ] Database is properly reset between tests
- [ ] Auth Admin user is seeded correctly
- [ ] All authentication tests pass
- [ ] Role management tests pass
- [ ] Multi-tenant workflow tests pass

## Additional Notes

### Test Execution Time
- Expected: 10-30 seconds for full suite
- Individual tests: < 500ms each

### Coverage Goals
- Line Coverage: > 80%
- Branch Coverage: > 70%
- Feature Coverage: 100% of critical paths

### Common Issues

**Issue**: Tests fail with "Auth Admin not found"
**Fix**: Ensure DatabaseSeeder runs in test environment

**Issue**: Concurrent test failures
**Fix**: Use IClassFixture for WebApplicationFactory sharing

**Issue**: Flaky tests
**Fix**: Ensure proper cleanup in IAsyncLifetime.DisposeAsync()

## Next Steps

1. Apply one of the database provider solutions above
2. Run the test suite: `dotnet test`
3. Verify all tests pass
4. Add to CI/CD pipeline
5. Set up code coverage reporting
6. Monitor test execution times
7. Add additional test scenarios as needed
