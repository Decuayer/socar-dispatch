using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Queries.GetTeamById;

public record GetTeamByIdQuery(Guid Id) : IRequest<ApiResponse<TeamDto>>;
