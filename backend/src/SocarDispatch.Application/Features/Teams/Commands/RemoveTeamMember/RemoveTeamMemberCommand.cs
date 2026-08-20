using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.RemoveTeamMember;

public record RemoveTeamMemberCommand(
    Guid TeamId,
    Guid RequesterId,
    Guid UserId
) : IRequest<ApiResponse<TeamDto>>;
