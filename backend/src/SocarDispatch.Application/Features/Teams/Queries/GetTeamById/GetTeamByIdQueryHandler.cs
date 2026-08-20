using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Teams.Queries.GetTeamById;

public class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, ApiResponse<TeamDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTeamByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TeamDto>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (team == null)
        {
            throw new EntityNotFoundException("Team", request.Id);
        }

        var dto = new TeamDto
        {
            Id = team.Id,
            TeamName = team.TeamName,
            Status = team.Status,
            LeaderId = team.LeaderId,
            LeaderFullName = team.Leader != null ? $"{team.Leader.FirstName} {team.Leader.LastName}".Trim() : null,
            CurrentLatitude = team.CurrentLatitude,
            CurrentLongitude = team.CurrentLongitude,
            UpdatedAt = team.UpdatedAt,
            Members = team.Members.Select(m => new TeamMemberDto
            {
                UserId = m.UserId,
                FullName = $"{m.User.FirstName} {m.User.LastName}".Trim(),
                Email = m.User.Email,
                Phone = m.User.Phone,
                Department = m.User.Department,
                SubRole = m.User.SubRole
            }).ToList()
        };

        return ApiResponse<TeamDto>.SuccessResult(dto, "Team details retrieved successfully.");
    }
}
