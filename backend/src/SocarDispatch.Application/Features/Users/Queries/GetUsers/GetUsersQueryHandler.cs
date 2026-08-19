
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Auth.DTOs;

namespace SocarDispatch.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ApiResponse<List<UserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                u.Phone.Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(u => u.Department.ToLower() == request.Department.Trim().ToLower());
        }

        if (request.RoleType.HasValue)
        {
            query = query.Where(u => u.RoleType == request.RoleType.Value);
        }

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Department = u.Department,
                RoleType = u.RoleType,
                SubRole = u.SubRole,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<UserDto>>.SuccessResult(users, "User directory list retrieved successfully.");
    }
}
