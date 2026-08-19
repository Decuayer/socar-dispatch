using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;

namespace SocarDispatch.Application.Features.Incidents.Queries.GetIncidentById;

public record GetIncidentByIdQuery(Guid Id) : IRequest<ApiResponse<IncidentDto>>;
