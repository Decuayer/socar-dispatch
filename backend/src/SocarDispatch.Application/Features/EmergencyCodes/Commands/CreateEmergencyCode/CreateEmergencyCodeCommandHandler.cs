using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.CreateEmergencyCode;

public class CreateEmergencyCodeCommandHandler : IRequestHandler<CreateEmergencyCodeCommand, ApiResponse<EmergencyCodeDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateEmergencyCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<EmergencyCodeDto>> Handle(CreateEmergencyCodeCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.EmergencyCodes
            .AnyAsync(c => c.Code == request.Code, cancellationToken);

        if (exists)
        {
            throw new DomainException($"Emergency code '{request.Code}' already exists.");
        }

        var entity = new EmergencyCodeDefinition
        {
            Code = request.Code,
            ColorHex = request.ColorHex,
            Description = request.Description,
            SeverityLevel = request.SeverityLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmergencyCodes.Add(entity);
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

        return ApiResponse<EmergencyCodeDto>.SuccessResult(dto, "Emergency code created successfully.");
    }
}
