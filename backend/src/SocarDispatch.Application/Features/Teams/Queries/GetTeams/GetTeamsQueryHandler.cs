using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;

namespace SocarDispatch.Application.Features.Teams.Queries.GetTeams;

public class GetTeamsQueryHandler : IRequestHandler<GetTeamsQuery, ApiResponse<List<TeamDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTeamsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TeamDto>>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var list = teams.Select(t => new TeamDto
        {
            Id = t.Id,
            TeamName = t.TeamName,
            Status = t.Status.ToString(),
            LeaderId = t.LeaderId,
            LeaderFullName = t.Leader != null ? $"{t.Leader.FirstName} {t.Leader.LastName}".Trim() : null,
            CurrentLatitude = t.CurrentLatitude,
            CurrentLongitude = t.CurrentLongitude,
            UpdatedAt = t.UpdatedAt,
            Members = t.Members.Select(m => new TeamMemberDto
            {
                UserId = m.UserId,
                FullName = $"{m.User.FirstName} {m.User.LastName}".Trim(),
                Email = m.User.Email,
                Phone = m.User.Phone,
                Department = m.User.Department,
                SubRole = m.User.SubRole,
                MemberStatus = m.MemberStatus.ToString(),
                StatusUpdatedAt = m.StatusUpdatedAt,
                JoinedAt = m.JoinedAt
            }).ToList()
        }).ToList();

        return ApiResponse<List<TeamDto>>.SuccessResult(list, "Teams retrieved successfully.");
    }
}
