using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Assignments.Commands.CreateAssignment;
using SocarDispatch.Application.Features.Assignments.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Operator")] // Only Dispatch Operators can assign teams.
public class AssignmentsController : ControllerBase
{
    private readonly ISender _sender;

    public AssignmentsController(ISender sender)
    {
        _sender = sender;
    }

    // POST /api/v1/assignments
    // Assigns an emergency response team to an incident.  
    // This action sets the incident status to 'Assigned' and the team status to 'Forwarded'.
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> CreateAssignment([FromBody] CreateAssignmentRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var operatorId))
        {
            throw new DomainException("Invalid user session. Token is missing or invalid.");
        }

        var command = new CreateAssignmentCommand(request.IncidentId, request.TeamId, operatorId);
        var result = await _sender.Send(command);
        return StatusCode(201, result);
    }
}
