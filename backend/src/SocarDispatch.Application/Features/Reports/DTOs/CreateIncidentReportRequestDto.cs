namespace SocarDispatch.Application.Features.Reports.DTOs;

public class CreateIncidentReportRequestDto
{
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public Guid? TeamId { get; set; }
}
