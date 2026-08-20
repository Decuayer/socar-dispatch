using MediatR;
using SocarDispatch.Application.Common.Models;

namespace SocarDispatch.Application.Features.Users.Commands.UpdateDeviceToken;

public record UpdateDeviceTokenCommand(
    Guid UserId,
    string Token
) : IRequest<ApiResponse<string>>;
