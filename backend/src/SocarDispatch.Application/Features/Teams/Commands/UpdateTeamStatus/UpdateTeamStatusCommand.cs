using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;

public record UpdateTeamStatusCommand(Guid TeamId, string Status) : IRequest<ApiResponse<TeamDto>>;
