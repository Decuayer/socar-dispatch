using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;

namespace SocarDispatch.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public record ChangeIncidentStatusCommand(
    Guid Id,
    string Status,
    Guid RequesterId,
    string RequesterRole
) : IRequest<ApiResponse<IncidentDto>>;
