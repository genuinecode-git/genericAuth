using GenericAuth.Domain.Entities;
using GenericAuth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationEntity = GenericAuth.Domain.Entities.Application;

namespace GenericAuth.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        // ApplicationCode value object
        builder.OwnsOne(a => a.Code, code =>
        {
            code.Property(c => c.Value)
                .HasColumnName("Code")
                .IsRequired()
                .HasMaxLength(50);

            code.HasIndex(c => c.Value)
                .IsUnique();
        });

        // ApiKey value object
        builder.OwnsOne(a => a.ApiKey, apiKey =>
        {
            apiKey.Property(k => k.HashedValue)
                .HasColumnName("ApiKeyHash")
                .IsRequired()
                .HasMaxLength(500);

            apiKey.Property(k => k.GeneratedAt)
                .HasColumnName("ApiKeyGeneratedAt")
                .IsRequired();
        });

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(255);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(255);

        // Relationships
        builder.HasMany(a => a.Roles)
            .WithOne(r => r.Application)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.UserApplications)
            .WithOne(ua => ua.Application)
            .HasForeignKey(ua => ua.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}
