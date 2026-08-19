using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Phone).HasMaxLength(20).IsRequired();
        builder.HasIndex(u => u.Phone).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Department).HasMaxLength(100).IsRequired();
        builder.Property(u => u.RoleType).HasMaxLength(20).IsRequired();
        builder.Property(u => u.SubRole).HasMaxLength(50);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
    }
}