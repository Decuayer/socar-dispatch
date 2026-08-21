using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.IncidentCategories.DTOs;

namespace SocarDispatch.Application.Features.IncidentCategories.Queries.GetIncidentCategories;

public record GetIncidentCategoriesQuery : IRequest<ApiResponse<List<IncidentCategoryDto>>>;
