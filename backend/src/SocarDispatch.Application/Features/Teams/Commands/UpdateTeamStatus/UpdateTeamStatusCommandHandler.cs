using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;

public class UpdateTeamStatusCommandHandler : IRequestHandler<UpdateTeamStatusCommand, ApiResponse<TeamDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateTeamStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TeamDto>> Handle(UpdateTeamStatusCommand request, CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team == null)
        {
            throw new EntityNotFoundException("Team", request.TeamId);
        }

        team.Status = request.Status;
        team.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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

        return ApiResponse<TeamDto>.SuccessResult(dto, "Team status updated successfully.");
    }
}
