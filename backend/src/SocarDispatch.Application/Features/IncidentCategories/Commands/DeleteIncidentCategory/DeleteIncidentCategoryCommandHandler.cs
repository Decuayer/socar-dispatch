using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.IncidentCategories.Commands.DeleteIncidentCategory;

public class DeleteIncidentCategoryCommandHandler : IRequestHandler<DeleteIncidentCategoryCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteIncidentCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteIncidentCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.IncidentCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new EntityNotFoundException("IncidentCategory", request.Id);
        }

        // Soft Delete
        category.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Incident category deactivated successfully.");
    }
}
