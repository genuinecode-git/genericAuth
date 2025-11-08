using GenericAuth.Domain.Entities;
using GenericAuth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericAuth.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);

            email.HasIndex(e => e.Value)
                .IsUnique();
        });

        // Password value object
        builder.OwnsOne(u => u.Password, password =>
        {
            password.Property(p => p.Hash)
                .HasColumnName("PasswordHash")
                .IsRequired()
                .HasMaxLength(500);
        });

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.IsEmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailConfirmationToken)
            .HasMaxLength(100);

        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.UserType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(100);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100);

        // RefreshTokens as owned collection
        builder.OwnsMany(u => u.RefreshTokens, refreshToken =>
        {
            refreshToken.ToTable("RefreshTokens");

            refreshToken.WithOwner().HasForeignKey("UserId");

            refreshToken.Property<int>("Id")
                .ValueGeneratedOnAdd();

            refreshToken.HasKey("Id");

            refreshToken.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            refreshToken.Property(rt => rt.ExpiresAt)
                .IsRequired();

            refreshToken.Property(rt => rt.CreatedAt)
                .IsRequired();

            refreshToken.Property(rt => rt.RevokedAt);

            refreshToken.Property(rt => rt.ReplacedByToken)
                .HasMaxLength(500);

            refreshToken.HasIndex(rt => rt.Token);
        });

        // Relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserApplications)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
