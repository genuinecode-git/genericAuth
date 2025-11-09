using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GenericAuth.API.IntegrationTests.Infrastructure;

/// <summary>
/// Custom web application factory for integration tests.
/// Configures the application to use an in-memory database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the actual database context registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                           d.ServiceType == typeof(ApplicationDbContext) ||
                           d.ServiceType == typeof(IApplicationDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.EnableSensitiveDataLogging();
            });

            // Re-register DbContext as IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());
        });

        // Configure test environment
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Creates a scope to access services for test setup.
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// Resets the in-memory database.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Delete and recreate the database
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Re-seed the database
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
