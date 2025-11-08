using GenericAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericAuth.Infrastructure.Persistence.Configurations;

public class UserApplicationConfiguration : IEntityTypeConfiguration<UserApplication>
{
    public void Configure(EntityTypeBuilder<UserApplication> builder)
    {
        builder.ToTable("UserApplications");

        // Composite primary key
        builder.HasKey(ua => new { ua.UserId, ua.ApplicationId });

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.Property(ua => ua.ApplicationId)
            .IsRequired();

        builder.Property(ua => ua.ApplicationRoleId)
            .IsRequired();

        builder.Property(ua => ua.AssignedAt)
            .IsRequired();

        builder.Property(ua => ua.AssignedBy)
            .HasMaxLength(255);

        builder.Property(ua => ua.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ua => ua.LastAccessedAt);

        builder.Property(ua => ua.UpdatedAt);

        // Relationships
        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserApplications)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Application)
            .WithMany(a => a.UserApplications)
            .HasForeignKey(ua => ua.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.ApplicationRole)
            .WithMany()
            .HasForeignKey(ua => ua.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete when role is deleted

        // Indexes
        builder.HasIndex(ua => ua.UserId);
        builder.HasIndex(ua => ua.ApplicationId);
        builder.HasIndex(ua => ua.ApplicationRoleId);
    }
}
