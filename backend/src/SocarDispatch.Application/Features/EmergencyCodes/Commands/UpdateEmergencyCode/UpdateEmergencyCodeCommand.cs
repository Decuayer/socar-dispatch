using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.UpdateEmergencyCode;

public record UpdateEmergencyCodeCommand(
    Guid Id,
    string Code,
    string ColorHex,
    string Description,
    int SeverityLevel,
    bool IsActive
) : IRequest<ApiResponse<EmergencyCodeDto>>;
