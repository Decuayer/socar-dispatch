using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.TeamName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Idle").IsRequired();
        
        // PostGIS Geometry Point (WGS84 - SRID 4326) and GiST Spatial Index
        builder.Property(t => t.Location).HasColumnType("geometry(Point, 4326)");
        builder.HasIndex(t => t.Location).HasMethod("GIST");

        builder.Property(t => t.UpdatedAt).HasDefaultValueSql("NOW()");

        // Team Leader (User -> Teams 1-to-Many optional relationship)
        builder.HasOne(t => t.Leader)
            .WithMany(u => u.LedTeams)
            .HasForeignKey(t => t.LeaderId)
            .OnDelete(DeleteBehavior.SetNull); // If the leader is deleted, the team is not deleted; the leader position becomes vacant.
    }
}