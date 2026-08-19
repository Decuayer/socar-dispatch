using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Incidents.DTOs;

namespace SocarDispatch.Application.Features.Incidents.Queries.GetIncidents;

public class GetIncidentsQueryHandler : IRequestHandler<GetIncidentsQuery, ApiResponse<List<IncidentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<IncidentDto>>> Handle(GetIncidentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Incidents
            .Include(i => i.Reporter)
            .Include(i => i.Assignments)
                .ThenInclude(a => a.Team)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(i => i.Status.ToLower() == request.Status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(i => i.Category.ToLower() == request.Category.ToLower());
        }

        var list = await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new IncidentDto
            {
                Id = i.Id,
                ReporterId = i.ReporterId,
                ReporterFullName = $"{i.Reporter.FirstName} {i.Reporter.LastName}".Trim(),
                Category = i.Category,
                EmergencyCode = i.EmergencyCode,
                Description = i.Description,
                MediaUrl = i.MediaUrl,
                Status = i.Status,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                CreatedAt = i.CreatedAt,
                AssignedTeamId = i.Assignments.OrderByDescending(a => a.AssignedAt).Select(a => (Guid?)a.TeamId).FirstOrDefault(),
                AssignedTeamName = i.Assignments.OrderByDescending(a => a.AssignedAt).Select(a => a.Team.TeamName).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<IncidentDto>>.SuccessResult(list, "Incidents retrieved successfully.");
    }
}
