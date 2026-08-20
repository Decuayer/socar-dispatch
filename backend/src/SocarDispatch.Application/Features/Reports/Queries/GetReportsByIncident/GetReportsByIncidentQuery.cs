using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Reports.DTOs;

namespace SocarDispatch.Application.Features.Reports.Queries.GetReportsByIncident;

public record GetReportsByIncidentQuery(Guid IncidentId) : IRequest<ApiResponse<List<IncidentReportDto>>>;
