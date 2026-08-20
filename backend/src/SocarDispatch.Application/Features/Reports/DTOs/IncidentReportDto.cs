namespace SocarDispatch.Application.Features.Reports.DTOs;

public class IncidentReportDto
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public Guid ReportedByUserId { get; set; }
    public string ReportedByFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime ReportedAt { get; set; }
}
