using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence.Configurations;

public class IncidentCategoryConfiguration : IEntityTypeConfiguration<IncidentCategory>
{
    public void Configure(EntityTypeBuilder<IncidentCategory> builder)
    {
        builder.ToTable("Incident_Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(250);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Seed Default Incident Categories
        builder.HasData(
            new IncidentCategory
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                Code = "Fire",
                Name = "Fire Emergency",
                Description = "Fire and combustion incidents",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new IncidentCategory
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                Code = "Medical",
                Name = "Medical Emergency",
                Description = "Medical emergencies and injuries",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new IncidentCategory
            {
                Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                Code = "Security",
                Name = "Security Incident",
                Description = "Physical and facility security incidents",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new IncidentCategory
            {
                Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                Code = "Environmental",
                Name = "Environmental Hazard",
                Description = "Environmental contamination and spills",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new IncidentCategory
            {
                Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"),
                Code = "Chemical",
                Name = "Chemical Incident",
                Description = "Chemical leaks and toxic substance exposure",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
