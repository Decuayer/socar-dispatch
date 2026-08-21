using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.IncidentCategories.Commands.CreateIncidentCategory;
using SocarDispatch.Application.Features.IncidentCategories.Commands.DeleteIncidentCategory;
using SocarDispatch.Application.Features.IncidentCategories.Commands.UpdateIncidentCategory;
using SocarDispatch.Application.Features.IncidentCategories.DTOs;
using SocarDispatch.Application.Features.IncidentCategories.Queries.GetIncidentCategories;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/incident-categories")]
[Authorize]
public class IncidentCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public IncidentCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    // GET /api/v1/incident-categories
    // Retrieves all active incident categories (Employee, Team, Operator)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<IncidentCategoryDto>>>> GetAll()
    {
        var query = new GetIncidentCategoriesQuery();
        var result = await _sender.Send(query);
        return Ok(result);
    }

    // POST /api/v1/incident-categories
    // Creates a new incident category definition (Operator only)
    [HttpPost]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<ApiResponse<IncidentCategoryDto>>> Create([FromBody] CreateIncidentCategoryRequestDto request)
    {
        var command = new CreateIncidentCategoryCommand(
            request.Code,
            request.Name,
            request.Description
        );

        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Data.Id }, result);
    }

    // PUT /api/v1/incident-categories/{id}
    // Updates an existing incident category definition (Operator only)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<ApiResponse<IncidentCategoryDto>>> Update(Guid id, [FromBody] UpdateIncidentCategoryRequestDto request)
    {
        var command = new UpdateIncidentCategoryCommand(
            id,
            request.Code,
            request.Name,
            request.Description,
            request.IsActive
        );

        var result = await _sender.Send(command);
        return Ok(result);
    }

    // DELETE /api/v1/incident-categories/{id}
    // Deactivates (soft-delete) an incident category definition (Operator only)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var command = new DeleteIncidentCategoryCommand(id);
        var result = await _sender.Send(command);
        return Ok(result);
    }
}
