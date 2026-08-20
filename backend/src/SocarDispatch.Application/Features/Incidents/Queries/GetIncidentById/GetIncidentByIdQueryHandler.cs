using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Incidents.Queries.GetIncidentById;

public class GetIncidentByIdQueryHandler : IRequestHandler<GetIncidentByIdQuery, ApiResponse<IncidentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IncidentDto>> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .Include(i => i.Reporter)
            .Include(i => i.Assignments)
                .ThenInclude(a => a.Team)
            .Include(i => i.MediaAttachments)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
        {
            throw new EntityNotFoundException("Incident", request.Id);
        }

        var latestAssignment = incident.Assignments.OrderByDescending(a => a.AssignedAt).FirstOrDefault();

        var dto = new IncidentDto
        {
            Id = incident.Id,
            ReporterId = incident.ReporterId,
            ReporterFullName = $"{incident.Reporter.FirstName} {incident.Reporter.LastName}".Trim(),
            Category = incident.Category,
            EmergencyCode = incident.EmergencyCode,
            Description = incident.Description,
            MediaAttachments = incident.MediaAttachments.Select(m => new IncidentMediaDto
            {
                Id = m.Id,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                CreatedAt = m.CreatedAt
            }).ToList(),
            Status = incident.Status,
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            CreatedAt = incident.CreatedAt,
            AssignedTeamId = latestAssignment?.TeamId,
            AssignedTeamName = latestAssignment?.Team.TeamName
        };

        return ApiResponse<IncidentDto>.SuccessResult(dto, "Incident details retrieved.");
    }
}
