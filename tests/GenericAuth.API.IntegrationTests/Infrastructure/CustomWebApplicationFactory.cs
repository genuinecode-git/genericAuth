using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GenericAuth.API.IntegrationTests.Infrastructure;

/// <summary>
/// Custom web application factory for integration tests.
/// Configures the application to use an in-memory SQLite database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configure test environment FIRST before services are built
        // This prevents production SQLite registration in Infrastructure layer
        builder.UseEnvironment("Testing");

        // Use ConfigureServices to register SQLite in-memory database
        // Since we're in Testing environment, Infrastructure won't register its SQLite
        builder.ConfigureServices(services =>
        {
            // Create and open the SQLite in-memory connection
            // We must keep the connection open for the lifetime of the test
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add SQLite in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                // Suppress pending model changes warning for tests
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            // Register DbContext as IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Creates a scope to access services for test setup.
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// Resets the in-memory SQLite database.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // For in-memory SQLite, we use EnsureCreated instead of Migrate
        // EnsureCreated creates the schema directly from the model
        await context.Database.EnsureCreatedAsync();

        // Clear all data
        context.RemoveRange(context.UserApplications);
        context.RemoveRange(context.ApplicationRoles);
        context.RemoveRange(context.Applications);
        context.RemoveRange(context.Roles);
        context.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        // Re-seed the database
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
