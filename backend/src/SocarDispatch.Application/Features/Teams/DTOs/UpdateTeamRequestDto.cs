namespace SocarDispatch.Application.Features.Teams.DTOs;

public record UpdateTeamRequestDto(
    string TeamName,
    Guid? LeaderId
);
