using GenericAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    // Multi-tenant application entities
    DbSet<Domain.Entities.Application> Applications { get; }
    DbSet<ApplicationRole> ApplicationRoles { get; }
    DbSet<UserApplication> UserApplications { get; }
    DbSet<ApplicationRolePermission> ApplicationRolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
