using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Reports.Commands.CreateReport;
using SocarDispatch.Application.Features.Reports.DTOs;
using SocarDispatch.Application.Features.Reports.Queries.GetReportsByIncident;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/incidents/{incidentId:guid}/reports")]
[Authorize]
public class IncidentReportsController : ControllerBase
{
    private readonly ISender _sender;

    public IncidentReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// POST /api/v1/incidents/{incidentId}/reports
    /// Adds a field report and media (MinIO URL) to the emergency incident.
    /// Authorization: Only a team member actively assigned to the relevant incident.
    [HttpPost]
    public async Task<ActionResult<ApiResponse<IncidentReportDto>>> CreateReport(
        [FromRoute] Guid incidentId,
        [FromBody] CreateIncidentReportRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var reportedByUserId))
        {
            throw new DomainException("Invalid user session. Token is missing or invalid.");
        }

        var command = new CreateIncidentReportCommand(
            incidentId,
            reportedByUserId,
            request.Content,
            request.MediaUrl,
            request.TeamId
        );

        var result = await _sender.Send(command);
        return Ok(result);
    }

    /// GET /api/v1/incidents/{incidentId}/reports
    /// Retrieves all field reports for the specified incident in chronological order.
    /// Authorization: Authenticated authorized users.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<IncidentReportDto>>>> GetReports([FromRoute] Guid incidentId)
    {
        var query = new GetReportsByIncidentQuery(incidentId);
        var result = await _sender.Send(query);
        return Ok(result);
    }
}
