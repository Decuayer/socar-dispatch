namespace SocarDispatch.Application.Features.IncidentCategories.DTOs;

public record UpdateIncidentCategoryRequestDto(
    string Code,
    string Name,
    string Description,
    bool IsActive
);
