using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Queries.GetTeams;

public record GetTeamsQuery() : IRequest<ApiResponse<List<TeamDto>>>;
