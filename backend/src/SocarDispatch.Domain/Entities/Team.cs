using NetTopologySuite.Geometries;

namespace SocarDispatch.Domain.Entities;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TeamName { get; set; } = string.Empty;
    public string Status { get; set; } = "Idle"; // Idle, Forwarded, OnScene, Busy
    
    // PostGIS Spatial Point (SRID 4326 - WGS84 GPS Coordinates)
    public Point? Location { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}