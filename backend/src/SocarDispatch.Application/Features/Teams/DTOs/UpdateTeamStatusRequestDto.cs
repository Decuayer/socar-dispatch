namespace SocarDispatch.Application.Features.Teams.DTOs;

public class UpdateTeamStatusRequestDto
{
    public Guid TeamId { get; set; }
    public string Status { get; set; } = string.Empty;
}
