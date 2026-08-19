namespace SocarDispatch.Application.Features.Incidents.DTOs;

public class UpdateIncidentRequestDto
{
    public string Category { get; set; } = string.Empty;
    public string EmergencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MediaUrl { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}