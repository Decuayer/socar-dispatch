using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.Commands.CreateEmergencyCode;
using SocarDispatch.Application.Features.EmergencyCodes.Commands.DeleteEmergencyCode;
using SocarDispatch.Application.Features.EmergencyCodes.Commands.UpdateEmergencyCode;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;
using SocarDispatch.Application.Features.EmergencyCodes.Queries.GetEmergencyCodes;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/emergency-codes")]
[Authorize]
public class EmergencyCodesController : ControllerBase
{
    private readonly ISender _sender;

    public EmergencyCodesController(ISender sender)
    {
        _sender = sender;
    }

    // GET /api/v1/emergency-codes
    // Retrieves all active emergency codes ordered by severity level (Employee, Team, Operator)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<EmergencyCodeDto>>>> GetAll()
    {
        var query = new GetEmergencyCodesQuery();
        var result = await _sender.Send(query);
        return Ok(result);
    }

    // POST /api/v1/emergency-codes
    // Creates a new emergency code definition (Operator only)
    [HttpPost]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<ApiResponse<EmergencyCodeDto>>> Create([FromBody] CreateEmergencyCodeRequestDto request)
    {
        var command = new CreateEmergencyCodeCommand(
            request.Code,
            request.ColorHex,
            request.Description,
            request.SeverityLevel
        );

        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Data.Id }, result);
    }

    // PUT /api/v1/emergency-codes/{id}
    // Updates an existing emergency code definition (Operator only)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<ApiResponse<EmergencyCodeDto>>> Update(Guid id, [FromBody] UpdateEmergencyCodeRequestDto request)
    {
        var command = new UpdateEmergencyCodeCommand(
            id,
            request.Code,
            request.ColorHex,
            request.Description,
            request.SeverityLevel,
            request.IsActive
        );

        var result = await _sender.Send(command);
        return Ok(result);
    }

    // DELETE /api/v1/emergency-codes/{id}
    // Deactivates (soft-delete) an emergency code definition (Operator only)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var command = new DeleteEmergencyCodeCommand(id);
        var result = await _sender.Send(command);
        return Ok(result);
    }
}
