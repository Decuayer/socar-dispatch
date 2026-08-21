using MediatR;
using SocarDispatch.Application.Common.Models;

namespace SocarDispatch.Application.Features.IncidentCategories.Commands.DeleteIncidentCategory;

public record DeleteIncidentCategoryCommand(Guid Id) : IRequest<ApiResponse<bool>>;
