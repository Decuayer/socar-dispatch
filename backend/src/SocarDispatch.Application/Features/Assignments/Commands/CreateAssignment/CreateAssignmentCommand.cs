using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Assignments.DTOs;

namespace SocarDispatch.Application.Features.Assignments.Commands.CreateAssignment;

public record CreateAssignmentCommand(Guid IncidentId, Guid TeamId, Guid OperatorId) : IRequest<ApiResponse<AssignmentDto>>;
