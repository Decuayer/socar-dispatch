namespace SocarDispatch.Application.Features.IncidentCategories.DTOs;

public record CreateIncidentCategoryRequestDto(
    string Code,
    string Name,
    string Description
);
