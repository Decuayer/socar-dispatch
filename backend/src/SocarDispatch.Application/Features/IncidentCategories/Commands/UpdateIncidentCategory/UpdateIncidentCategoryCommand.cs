using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.IncidentCategories.DTOs;

namespace SocarDispatch.Application.Features.IncidentCategories.Commands.UpdateIncidentCategory;

public record UpdateIncidentCategoryCommand(
    Guid Id,
    string Code,
    string Name,
    string Description,
    bool IsActive
) : IRequest<ApiResponse<IncidentCategoryDto>>;
