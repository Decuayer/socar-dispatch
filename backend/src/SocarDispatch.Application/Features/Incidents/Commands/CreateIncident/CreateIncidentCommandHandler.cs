using MediatR;
using NetTopologySuite.Geometries;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Incidents.Commands.CreateIncident;



public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, ApiResponse<IncidentDto>>
{
    private readonly IApplicationDbContext _context;
    public CreateIncidentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<ApiResponse<IncidentDto>> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var reporter = await _context.Users.FindAsync(new object[] { request.ReporterId }, cancellationToken);
        if (reporter == null)
        {
            throw new EntityNotFoundException("User", request.ReporterId);
        }
        var incident = new Incident
        {
            ReporterId = request.ReporterId,
            Category = request.Category,
            EmergencyCode = request.EmergencyCode,
            Description = request.Description,
            MediaUrl = request.MediaUrl,
            Status = "Open",
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Location = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 },
            CreatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);
        var dto = new IncidentDto
        {
            Id = incident.Id,
            ReporterId = reporter.Id,
            ReporterFullName = $"{reporter.FirstName} {reporter.LastName}".Trim(),
            Category = incident.Category,
            EmergencyCode = incident.EmergencyCode,
            Description = incident.Description,
            MediaUrl = incident.MediaUrl,
            Status = incident.Status,
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            CreatedAt = incident.CreatedAt
        };
        return ApiResponse<IncidentDto>.SuccessResult(dto, "Incident successfully reported.");
    }
}