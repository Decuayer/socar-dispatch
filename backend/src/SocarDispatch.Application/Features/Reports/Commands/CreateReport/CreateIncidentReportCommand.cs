using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Reports.DTOs;

namespace SocarDispatch.Application.Features.Reports.Commands.CreateReport;

public record CreateIncidentReportCommand(
    Guid IncidentId,
    Guid ReportedByUserId,
    string Content,
    string? MediaUrl = null,
    Guid? TeamId = null
) : IRequest<ApiResponse<IncidentReportDto>>;
