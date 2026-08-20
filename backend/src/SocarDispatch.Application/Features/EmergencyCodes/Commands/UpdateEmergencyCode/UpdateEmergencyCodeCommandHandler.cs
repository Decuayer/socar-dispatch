using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.UpdateEmergencyCode;

public class UpdateEmergencyCodeCommandHandler : IRequestHandler<UpdateEmergencyCodeCommand, ApiResponse<EmergencyCodeDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmergencyCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<EmergencyCodeDto>> Handle(UpdateEmergencyCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.EmergencyCodes
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException("EmergencyCodeDefinition", request.Id);
        }

        var codeExists = await _context.EmergencyCodes
            .AnyAsync(c => c.Code == request.Code && c.Id != request.Id, cancellationToken);

        if (codeExists)
        {
            throw new DomainException($"Emergency code '{request.Code}' already exists.");
        }

        entity.Code = request.Code;
        entity.ColorHex = request.ColorHex;
        entity.Description = request.Description;
        entity.SeverityLevel = request.SeverityLevel;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmergencyCodeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            ColorHex = entity.ColorHex,
            Description = entity.Description,
            SeverityLevel = entity.SeverityLevel,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };

        return ApiResponse<EmergencyCodeDto>.SuccessResult(dto, "Emergency code updated successfully.");
    }
}
