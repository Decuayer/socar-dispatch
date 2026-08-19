namespace SocarDispatch.Application.Features.Assignments.DTOs;

public class CreateAssignmentRequestDto
{
    public Guid IncidentId { get; set; }
    public Guid TeamId { get; set; }
}
