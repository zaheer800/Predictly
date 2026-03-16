using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(u => u.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(u => u.DisplayName).HasColumnName("display_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasMaxLength(20)
            .HasConversion(
                r => r.ToString().ToLower(),
                r => Enum.Parse<UserRole>(r, ignoreCase: true))
            .HasDefaultValue(UserRole.User);

        builder.HasIndex(u => u.Email).HasDatabaseName("idx_users_email").IsUnique();
        builder.HasIndex(u => u.Username).HasDatabaseName("idx_users_username").IsUnique();
    }
}
