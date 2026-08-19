using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Category).HasMaxLength(50).IsRequired();
        builder.Property(i => i.EmergencyCode).HasMaxLength(20).IsRequired();
        builder.Property(i => i.Description).HasColumnType("text");
        builder.Property(i => i.MediaUrl).HasMaxLength(500);
        builder.Property(i => i.Status).HasMaxLength(20).HasDefaultValue("Open").IsRequired();

        builder.Property(t => t.Latitude).HasPrecision(9, 6);
        builder.Property(t => t.Longitude).HasPrecision(9, 6);

        // PostGIS Geometry Point and GiST Spatial Index
        builder.Property(i => i.Location).HasColumnType("geometry(Point, 4326)").IsRequired();
        builder.HasIndex(i => i.Location).HasMethod("GIST");

        builder.Property(i => i.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(i => i.Reporter)
            .WithMany(u => u.ReportedIncidents)
            .HasForeignKey(i => i.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}