using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Auth.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UsersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new DomainException("Invalid user session.");
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Department = user.Department,
            RoleType = user.RoleType,
            SubRole = user.SubRole,
            AvatarUrl = user.AvatarUrl
        };

        return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "User information successfully retrieved."));
    }

    [HttpGet("admin-only-test")]
    [Authorize(Roles = "Operator")]
    public ActionResult<ApiResponse<string>> OperatorOnlyEndpoint()
    {
        return Ok(ApiResponse<string>.SuccessResult(
            "Success! Only users with the Operator role can view this data.",
            "Authorization Approved"));
    }
}