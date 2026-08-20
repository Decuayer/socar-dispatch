using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.DeleteEmergencyCode;

public class DeleteEmergencyCodeCommandHandler : IRequestHandler<DeleteEmergencyCodeCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmergencyCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteEmergencyCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.EmergencyCodes
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException("EmergencyCodeDefinition", request.Id);
        }

        // Soft-delete
        entity.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Emergency code deactivated successfully.");
    }
}
