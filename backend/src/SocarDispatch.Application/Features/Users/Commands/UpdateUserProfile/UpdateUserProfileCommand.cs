using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Auth.DTOs;

namespace SocarDispatch.Application.Features.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Phone,
    string Department,
    string? SubRole,
    string? AvatarUrl
) : IRequest<ApiResponse<UserDto>>;
