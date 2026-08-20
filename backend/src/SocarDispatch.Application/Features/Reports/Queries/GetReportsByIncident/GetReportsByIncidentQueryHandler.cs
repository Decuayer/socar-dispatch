using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Reports.DTOs;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Reports.Queries.GetReportsByIncident;

public class GetReportsByIncidentQueryHandler : IRequestHandler<GetReportsByIncidentQuery, ApiResponse<List<IncidentReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetReportsByIncidentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<IncidentReportDto>>> Handle(GetReportsByIncidentQuery request, CancellationToken cancellationToken)
    {
        var incidentExists = await _context.Incidents.AnyAsync(i => i.Id == request.IncidentId, cancellationToken);
        if (!incidentExists)
        {
            throw new EntityNotFoundException("Incident", request.IncidentId);
        }

        var reports = await _context.IncidentReports
            .Include(r => r.Team)
            .Include(r => r.ReportedBy)
            .Where(r => r.IncidentId == request.IncidentId)
            .OrderBy(r => r.ReportedAt)
            .Select(r => new IncidentReportDto
            {
                Id = r.Id,
                IncidentId = r.IncidentId,
                TeamId = r.TeamId,
                TeamName = r.Team.TeamName,
                ReportedByUserId = r.ReportedByUserId,
                ReportedByFullName = (r.ReportedBy.FirstName + " " + r.ReportedBy.LastName).Trim(),
                Content = r.Content,
                MediaUrl = r.MediaUrl,
                ReportedAt = r.ReportedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return ApiResponse<List<IncidentReportDto>>.SuccessResult(reports, "Incident field reports retrieved successfully.");
    }
}
