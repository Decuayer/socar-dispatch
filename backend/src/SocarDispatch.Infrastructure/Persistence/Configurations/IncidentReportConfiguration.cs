using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence.Configurations;

public class IncidentReportConfiguration : IEntityTypeConfiguration<IncidentReport>
{
    public void Configure(EntityTypeBuilder<IncidentReport> builder)
    {
        builder.ToTable("Incident_Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.MediaUrl)
            .HasMaxLength(500);

        builder.Property(r => r.ReportedAt)
            .HasDefaultValueSql("NOW()");

        // İlişkiler ve Silme Davranışları (Foreign Keys & Delete Behaviors)
        // 1. Incident Silindiğinde bağlı raporlar da silinsin (Cascade)
        builder.HasOne(r => r.Incident)
            .WithMany(i => i.Reports)
            .HasForeignKey(r => r.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. Team silindiğinde bağlı raporlar korunsun/engellensin (Restrict)
        builder.HasOne(r => r.Team)
            .WithMany(t => t.Reports)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Kullanıcı silindiğinde bağlı raporlar korunsun/engellensin (Restrict)
        builder.HasOne(r => r.ReportedBy)
            .WithMany()
            .HasForeignKey(r => r.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
