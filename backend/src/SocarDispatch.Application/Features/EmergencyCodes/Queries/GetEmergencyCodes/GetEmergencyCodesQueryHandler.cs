using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;

namespace SocarDispatch.Application.Features.EmergencyCodes.Queries.GetEmergencyCodes;

public class GetEmergencyCodesQueryHandler : IRequestHandler<GetEmergencyCodesQuery, ApiResponse<List<EmergencyCodeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEmergencyCodesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<EmergencyCodeDto>>> Handle(GetEmergencyCodesQuery request, CancellationToken cancellationToken)
    {
        var codes = await _context.EmergencyCodes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SeverityLevel)
            .Select(c => new EmergencyCodeDto
            {
                Id = c.Id,
                Code = c.Code,
                ColorHex = c.ColorHex,
                Description = c.Description,
                SeverityLevel = c.SeverityLevel,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<EmergencyCodeDto>>.SuccessResult(codes);
    }
}
