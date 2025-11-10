using GenericAuth.Domain.Entities;
using GenericAuth.Domain.Enums;
using GenericAuth.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenericAuth.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with initial data including Auth Admin user.
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created (for production, migrations should be applied separately)
            // For in-memory test databases, the schema is created by EnsureCreated in the test factory
            // For production, migrations should be run before seeding
            if (_context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                await _context.Database.MigrateAsync();
            }

            // Seed Auth Admin user
            await SeedAuthAdminUserAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    /// <summary>
    /// Creates the default Auth Admin user if it doesn't exist.
    /// </summary>
    private async Task SeedAuthAdminUserAsync()
    {
        const string adminEmail = "admin@genericauth.com";
        const string defaultPassword = "Admin@123";

        // Check if Auth Admin already exists
        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == adminEmail && u.UserType == UserType.AuthAdmin);

        if (existingAdmin != null)
        {
            _logger.LogInformation("Auth Admin user already exists");
            return;
        }

        // Create Auth Admin user
        var passwordHash = _passwordHasher.Hash(defaultPassword);
        var authAdmin = User.CreateAuthAdmin(
            firstName: "System",
            lastName: "Administrator",
            email: adminEmail,
            passwordHash: passwordHash
        );

        // Confirm email for convenience in development
        authAdmin.ConfirmEmail();

        await _context.Users.AddAsync(authAdmin);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Auth Admin user created successfully. Email: {Email}, Password: {Password}",
            adminEmail,
            defaultPassword
        );
        _logger.LogWarning(
            "⚠️ SECURITY WARNING: Default Auth Admin password is '{Password}'. Change this immediately in production!",
            defaultPassword
        );
    }
}
