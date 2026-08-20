namespace SocarDispatch.Application.Features.Teams.DTOs;

public record CreateTeamRequestDto(
    string TeamName,
    Guid? LeaderId
);
