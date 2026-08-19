namespace SocarDispatch.Application.Features.Incidents.DTOs;

public class IncidentDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterFullName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EmergencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MediaUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Assigned Team Information (if any)
    public Guid? AssignedTeamId { get; set; }
    public string? AssignedTeamName { get; set; }
}