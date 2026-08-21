using MediatR;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Teams.DTOs;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Domain.Events;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamLocation;

public class UpdateTeamLocationCommandHandler : IRequestHandler<UpdateTeamLocationCommand, ApiResponse<TeamDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public UpdateTeamLocationCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<ApiResponse<TeamDto>> Handle(UpdateTeamLocationCommand request, CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team == null)
        {
            throw new EntityNotFoundException("Team", request.TeamId);
        }

        team.CurrentLatitude = request.Latitude;
        team.CurrentLongitude = request.Longitude;
        team.Location = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };
        team.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new TeamLocationUpdatedEvent(
            team.Id,
            (double)request.Latitude,
            (double)request.Longitude,
            team.UpdatedAt
        ), cancellationToken);

        var dto = new TeamDto
        {
            Id = team.Id,
            TeamName = team.TeamName,
            Status = team.Status.ToString(),
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

        return ApiResponse<TeamDto>.SuccessResult(dto, "Team real-time location updated.");
    }
}
