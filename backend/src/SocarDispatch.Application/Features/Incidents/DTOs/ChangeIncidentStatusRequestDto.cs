namespace SocarDispatch.Application.Features.Incidents.DTOs;

public class ChangeIncidentStatusRequestDto
{
    public string Status { get; set; } = string.Empty; // Open, Assigned, Resolved, Canceled
    public string? CompletionNotes { get; set; }
}
