using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamLocation;

public record UpdateTeamLocationCommand(Guid TeamId, decimal Latitude, decimal Longitude) : IRequest<ApiResponse<TeamDto>>;
