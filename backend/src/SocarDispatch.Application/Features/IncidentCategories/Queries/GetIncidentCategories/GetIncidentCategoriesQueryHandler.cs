using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.IncidentCategories.DTOs;

namespace SocarDispatch.Application.Features.IncidentCategories.Queries.GetIncidentCategories;

public class GetIncidentCategoriesQueryHandler : IRequestHandler<GetIncidentCategoriesQuery, ApiResponse<List<IncidentCategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<IncidentCategoryDto>>> Handle(GetIncidentCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.IncidentCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Select(c => new IncidentCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<IncidentCategoryDto>>.SuccessResult(categories);
    }
}
