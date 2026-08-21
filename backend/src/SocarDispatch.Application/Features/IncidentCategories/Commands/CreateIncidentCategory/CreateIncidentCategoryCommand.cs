using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.IncidentCategories.DTOs;

namespace SocarDispatch.Application.Features.IncidentCategories.Commands.CreateIncidentCategory;

public record CreateIncidentCategoryCommand(
    string Code,
    string Name,
    string Description
) : IRequest<ApiResponse<IncidentCategoryDto>>;
