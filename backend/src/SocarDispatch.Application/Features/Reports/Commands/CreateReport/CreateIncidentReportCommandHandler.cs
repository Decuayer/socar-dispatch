using MediatR;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Reports.DTOs;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Exceptions;

namespace SocarDispatch.Application.Features.Reports.Commands.CreateReport;

public class CreateIncidentReportCommandHandler : IRequestHandler<CreateIncidentReportCommand, ApiResponse<IncidentReportDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateIncidentReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IncidentReportDto>> Handle(CreateIncidentReportCommand request, CancellationToken cancellationToken)
    {
        // 1. Olayın varlığını doğrula
        var incident = await _context.Incidents.FindAsync(new object[] { request.IncidentId }, cancellationToken);
        if (incident == null)
        {
            throw new EntityNotFoundException("Incident", request.IncidentId);
        }

        // 2. Raporlayan kullanıcının ekibini (TeamId) belirle
        Guid targetTeamId;
        if (request.TeamId.HasValue && request.TeamId.Value != Guid.Empty)
        {
            targetTeamId = request.TeamId.Value;
        }
        else
        {
            var userTeamId = await _context.TeamMembers
                .Where(tm => tm.UserId == request.ReportedByUserId)
                .Select(tm => tm.TeamId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userTeamId == Guid.Empty)
            {
                userTeamId = await _context.Teams
                    .Where(t => t.LeaderId == request.ReportedByUserId)
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (userTeamId == Guid.Empty)
            {
                throw new ForbiddenAccessException("Your team is not actively assigned to this incident.");
            }

            targetTeamId = userTeamId;
        }

        // 3. İş Kuralı Doğrulaması (Business Rule): Ekibin bu olaya aktif olarak atanıp atanmadığını kontrol et
        var isAssigned = await _context.Assignments
            .AnyAsync(a => a.IncidentId == request.IncidentId
                           && a.TeamId == targetTeamId
                           && a.CompletedAt == null, cancellationToken);

        if (!isAssigned)
        {
            throw new ForbiddenAccessException("Your team is not actively assigned to this incident.");
        }

        // 4. Ekip ve Kullanıcı bilgilerini yükle
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == targetTeamId, cancellationToken);
        if (team == null)
        {
            throw new EntityNotFoundException("Team", targetTeamId);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReportedByUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", request.ReportedByUserId);
        }

        // 5. Rapor varlığını oluştur ve veritabanına kaydet
        var report = new IncidentReport
        {
            IncidentId = request.IncidentId,
            TeamId = targetTeamId,
            ReportedByUserId = request.ReportedByUserId,
            Content = request.Content,
            MediaUrl = request.MediaUrl,
            ReportedAt = DateTime.UtcNow
        };

        _context.IncidentReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Yanıt DTO'su döndür
        var dto = new IncidentReportDto
        {
            Id = report.Id,
            IncidentId = report.IncidentId,
            TeamId = report.TeamId,
            TeamName = team.TeamName,
            ReportedByUserId = report.ReportedByUserId,
            ReportedByFullName = $"{user.FirstName} {user.LastName}".Trim(),
            Content = report.Content,
            MediaUrl = report.MediaUrl,
            ReportedAt = report.ReportedAt
        };

        return ApiResponse<IncidentReportDto>.SuccessResult(dto, "Incident field report created successfully.");
    }
}
