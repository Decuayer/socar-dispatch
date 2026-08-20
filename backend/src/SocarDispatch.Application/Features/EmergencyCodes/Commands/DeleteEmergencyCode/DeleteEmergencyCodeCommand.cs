using MediatR;
using SocarDispatch.Application.Common.Models;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.DeleteEmergencyCode;

public record DeleteEmergencyCodeCommand(Guid Id) : IRequest<ApiResponse<bool>>;
