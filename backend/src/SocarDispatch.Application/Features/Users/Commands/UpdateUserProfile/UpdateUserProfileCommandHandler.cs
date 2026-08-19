using MediatR;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Auth.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", request.UserId);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.Department = request.Department;
        user.SubRole = request.SubRole;
        user.AvatarUrl = request.AvatarUrl;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Department = user.Department,
            RoleType = user.RoleType,
            SubRole = user.SubRole,
            AvatarUrl = user.AvatarUrl
        };

        return ApiResponse<UserDto>.SuccessResult(dto, "User profile updated successfully.");
    }
}
