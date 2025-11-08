using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenericAuth.Infrastructure.Persistence;

/// <summary>
/// Factory for creating ApplicationDbContext at design time (for migrations).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a temporary connection string for migrations
        // This will be replaced with the actual connection string at runtime
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GenericAuthDb;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
