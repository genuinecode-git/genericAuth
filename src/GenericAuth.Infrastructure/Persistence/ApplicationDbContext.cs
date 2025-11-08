using System.Reflection;
using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Domain.Common;
using GenericAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = GenericAuth.Domain.Entities.Application;

namespace GenericAuth.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Multi-tenant application entities
    public DbSet<ApplicationEntity> Applications => Set<ApplicationEntity>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<UserApplication> UserApplications => Set<UserApplication>();
    public DbSet<ApplicationRolePermission> ApplicationRolePermissions => Set<ApplicationRolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            // Note: In a real application, you would publish these events to MediatR
            // or a message bus. For now, we'll just clear them.
            // await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
