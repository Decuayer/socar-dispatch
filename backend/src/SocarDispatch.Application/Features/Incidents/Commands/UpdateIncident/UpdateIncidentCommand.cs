using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;

namespace SocarDispatch.Application.Features.Incidents.Commands.UpdateIncident;

public record UpdateIncidentCommand(
    Guid Id,
    string Category,
    string EmergencyCode,
    string? Description,
    List<CreateIncidentMediaRequestDto> MediaAttachments,
    decimal Latitude,
    decimal Longitude
) : IRequest<ApiResponse<IncidentDto>>;
