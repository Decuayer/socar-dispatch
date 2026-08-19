using MediatR;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Incidents.Commands.UpdateIncident;

public class UpdateIncidentCommandHandler : IRequestHandler<UpdateIncidentCommand, ApiResponse<IncidentDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateIncidentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IncidentDto>> Handle(UpdateIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .Include(i => i.Reporter)
            .Include(i => i.Assignments).ThenInclude(a => a.Team)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
        {
            throw new EntityNotFoundException("Incident", request.Id);
        }

        incident.Category = request.Category;
        incident.EmergencyCode = request.EmergencyCode;
        incident.Description = request.Description;
        incident.MediaUrl = request.MediaUrl;
        incident.Latitude = request.Latitude;
        incident.Longitude = request.Longitude;
        incident.Location = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };

        await _context.SaveChangesAsync(cancellationToken);

        var latestAssignment = incident.Assignments.OrderByDescending(a => a.AssignedAt).FirstOrDefault();

        var dto = new IncidentDto
        {
            Id = incident.Id,
            ReporterId = incident.ReporterId,
            ReporterFullName = $"{incident.Reporter.FirstName} {incident.Reporter.LastName}".Trim(),
            Category = incident.Category,
            EmergencyCode = incident.EmergencyCode,
            Description = incident.Description,
            MediaUrl = incident.MediaUrl,
            Status = incident.Status,
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            CreatedAt = incident.CreatedAt,
            AssignedTeamId = latestAssignment?.TeamId,
            AssignedTeamName = latestAssignment?.Team.TeamName
        };

        return ApiResponse<IncidentDto>.SuccessResult(dto, "Incident updated successfully.");
    }
}
