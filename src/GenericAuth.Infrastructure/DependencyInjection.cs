using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Domain.Interfaces;
using GenericAuth.Domain.Services;
using GenericAuth.Infrastructure.Identity;
using GenericAuth.Infrastructure.Persistence;
using GenericAuth.Infrastructure.Persistence.Repositories;
using GenericAuth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenericAuth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - EF Core with SQLite (cross-platform)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repository Pattern
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Dapper - Read-only queries
        services.AddScoped<IQueryDbConnection, DapperDbConnection>();

        // Identity & Authentication
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Services
        services.AddTransient<IDateTime, DateTimeService>();

        // Database Seeder
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
