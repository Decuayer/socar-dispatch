using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence.Configurations;

public class IncidentMediaConfiguration : IEntityTypeConfiguration<IncidentMedia>
{
    public void Configure(EntityTypeBuilder<IncidentMedia> builder)
    {
        builder.ToTable("Incident_Media");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.MediaUrl).HasMaxLength(500).IsRequired();
        builder.Property(m => m.MediaType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(m => m.Incident)
            .WithMany(i => i.MediaAttachments)
            .HasForeignKey(m => m.IncidentId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade Delete Rule
    }
}
