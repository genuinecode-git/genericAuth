using GenericAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericAuth.Infrastructure.Persistence.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRoles");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.Id)
            .ValueGeneratedNever();

        builder.Property(ar => ar.ApplicationId)
            .IsRequired();

        builder.Property(ar => ar.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ar => ar.Description)
            .HasMaxLength(500);

        builder.Property(ar => ar.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ar => ar.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ar => ar.CreatedAt)
            .IsRequired();

        builder.Property(ar => ar.UpdatedAt);

        builder.Property(ar => ar.CreatedBy)
            .HasMaxLength(255);

        builder.Property(ar => ar.UpdatedBy)
            .HasMaxLength(255);

        // Index for role lookup within an application
        builder.HasIndex(ar => new { ar.ApplicationId, ar.Name })
            .IsUnique();

        // Relationships
        builder.HasOne(ar => ar.Application)
            .WithMany(a => a.Roles)
            .HasForeignKey(ar => ar.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ar => ar.Permissions)
            .WithOne(arp => arp.ApplicationRole)
            .HasForeignKey(arp => arp.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(ar => ar.DomainEvents);
    }
}
