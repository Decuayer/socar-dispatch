using NetTopologySuite.Geometries;

namespace SocarDispatch.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReporterId { get; set; }
    public User Reporter { get; set; } = null!;

    public string Category { get; set; } = string.Empty;
    public string EmergencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MediaUrl { get; set; }
    public string Status { get; set; } = "Open"; // Open, Assigned, Resolved, Canceled
    
    // PostGIS Spatial Point (SRID 4326)
    public Point Location { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}