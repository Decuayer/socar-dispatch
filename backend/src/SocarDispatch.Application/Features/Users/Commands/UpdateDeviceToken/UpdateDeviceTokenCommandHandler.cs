using MediatR;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Users.Commands.UpdateDeviceToken;

public class UpdateDeviceTokenCommandHandler : IRequestHandler<UpdateDeviceTokenCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeviceTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(UpdateDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", request.UserId);
        }

        user.DeviceToken = request.Token;
        user.DeviceTokenUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.SuccessResult("Device token updated successfully.", "Device token updated.");
    }
}
