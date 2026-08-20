using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeam;

public record UpdateTeamCommand(
    Guid Id,
    Guid RequesterId,
    string TeamName,
    Guid? LeaderId
) : IRequest<ApiResponse<TeamDto>>;
