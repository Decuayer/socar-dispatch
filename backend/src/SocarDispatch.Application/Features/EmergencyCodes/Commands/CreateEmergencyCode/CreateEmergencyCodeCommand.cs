using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.CreateEmergencyCode;

public record CreateEmergencyCodeCommand(
    string Code,
    string ColorHex,
    string Description,
    int SeverityLevel
) : IRequest<ApiResponse<EmergencyCodeDto>>;
