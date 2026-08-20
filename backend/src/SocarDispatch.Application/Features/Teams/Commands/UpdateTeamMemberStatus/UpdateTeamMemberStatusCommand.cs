using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamMemberStatus;

public record UpdateTeamMemberStatusCommand(
    Guid TeamId,
    Guid TargetUserId,
    Guid RequesterId,
    string Status
) : IRequest<ApiResponse<TeamMemberDto>>;
