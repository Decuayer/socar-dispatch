using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.AddTeamMember;

public record AddTeamMemberCommand(
    Guid TeamId,
    Guid RequesterId,
    Guid UserId
) : IRequest<ApiResponse<TeamDto>>;
