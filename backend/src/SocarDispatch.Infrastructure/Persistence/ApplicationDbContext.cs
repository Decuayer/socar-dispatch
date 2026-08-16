using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Report the PostGIS extension to EF Core
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Automatically apply all IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}