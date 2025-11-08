using GenericAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericAuth.Infrastructure.Persistence.Configurations;

public class ApplicationRolePermissionConfiguration : IEntityTypeConfiguration<ApplicationRolePermission>
{
    public void Configure(EntityTypeBuilder<ApplicationRolePermission> builder)
    {
        builder.ToTable("ApplicationRolePermissions");

        // Composite primary key
        builder.HasKey(arp => new { arp.ApplicationRoleId, arp.PermissionId });

        builder.Property(arp => arp.ApplicationRoleId)
            .IsRequired();

        builder.Property(arp => arp.PermissionId)
            .IsRequired();

        builder.Property(arp => arp.AssignedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(arp => arp.ApplicationRole)
            .WithMany(ar => ar.Permissions)
            .HasForeignKey(arp => arp.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(arp => arp.Permission)
            .WithMany()
            .HasForeignKey(arp => arp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
