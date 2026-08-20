using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeamLocation;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;
using SocarDispatch.Application.Features.Teams.DTOs;
using SocarDispatch.Application.Features.Teams.Queries.GetTeams;
using System.Security.Claims;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ISender _sender;

    public TeamsController(ISender sender)
    {
        _sender = sender;
    }

    // GET /api/v1/teams
    // Retrieves all emergency teams, their members, and their real-time locations and statuses.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeamDto>>>> GetAll()
    {
        var query = new GetTeamsQuery();
        var result = await _sender.Send(query);
        return Ok(result);
    }

    // PATCH /api/v1/teams/{teamId}/status
    // Updates the status of the emergency team (Idle, Forwarded, OnScene, Busy).
    [HttpPatch("{teamId:guid}/status")]
    [Authorize(Roles = "Operator,Team")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> UpdateStatus(Guid teamId, [FromBody] UpdateTeamStatusRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var requesterId))
        {
            throw new DomainException("Invalid user session. Token is missing or invalid.");
        }
        var command = new UpdateTeamStatusCommand(teamId, requesterId, request.Status);
        var result = await _sender.Send(command);
        return Ok(result);
    }

    // POST /api/v1/teams/location
    // Updates the emergency team's real-time GPS location data (Latitude/Longitude) to the server.
    [HttpPost("location")]
    [Authorize(Roles = "Operator,Team")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> UpdateLocation([FromBody] UpdateTeamLocationRequestDto request)
    {
        var command = new UpdateTeamLocationCommand(request.TeamId, request.Latitude, request.Longitude);
        var result = await _sender.Send(command);
        return Ok(result);
    }
    

}