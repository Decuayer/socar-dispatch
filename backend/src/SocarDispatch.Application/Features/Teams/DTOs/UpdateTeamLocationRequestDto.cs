namespace SocarDispatch.Application.Features.Teams.DTOs;

public class UpdateTeamLocationRequestDto
{
    public Guid TeamId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
