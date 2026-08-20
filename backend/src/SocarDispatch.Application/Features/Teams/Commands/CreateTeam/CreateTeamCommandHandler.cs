using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Enums;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Teams.Commands.CreateTeam;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, ApiResponse<TeamDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateTeamCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.Teams
            .AnyAsync(t => t.TeamName.ToLower() == request.TeamName.ToLower(), cancellationToken);
        if (nameExists)
        {
            throw new DomainException($"A team with name '{request.TeamName}' already exists.");
        }

        if (request.LeaderId.HasValue && request.LeaderId.Value != Guid.Empty)
        {
            var leader = await _context.Users.FindAsync(new object[] { request.LeaderId.Value }, cancellationToken);
            if (leader == null)
            {
                throw new EntityNotFoundException("User", request.LeaderId.Value);
            }

            if (leader.RoleType != RoleType.Team)
            {
                throw new DomainException("Designated team leader must have RoleType 'Team'.");
            }
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            TeamName = request.TeamName,
            LeaderId = request.LeaderId,
            Status = TeamStatus.Idle,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.LeaderId.HasValue && request.LeaderId.Value != Guid.Empty)
        {
            team.Members.Add(new TeamMember
            {
                TeamId = team.Id,
                UserId = request.LeaderId.Value
            });
        }

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);

        var createdTeam = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstAsync(t => t.Id == team.Id, cancellationToken);

        var dto = MapToTeamDto(createdTeam);
        return ApiResponse<TeamDto>.SuccessResult(dto, "Team created successfully.");
    }

    private static TeamDto MapToTeamDto(Team t)
    {
        return new TeamDto
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
                SubRole = m.User.SubRole
            }).ToList()
        };
    }
}
