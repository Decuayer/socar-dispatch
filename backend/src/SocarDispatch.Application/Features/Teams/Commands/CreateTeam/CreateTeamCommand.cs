using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.CreateTeam;

public record CreateTeamCommand(
    string TeamName,
    Guid? LeaderId,
    Guid RequesterId
) : IRequest<ApiResponse<TeamDto>>;
