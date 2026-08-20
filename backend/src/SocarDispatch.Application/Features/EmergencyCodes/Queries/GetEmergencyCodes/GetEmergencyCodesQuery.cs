using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.EmergencyCodes.DTOs;

namespace SocarDispatch.Application.Features.EmergencyCodes.Queries.GetEmergencyCodes;

public record GetEmergencyCodesQuery() : IRequest<ApiResponse<List<EmergencyCodeDto>>>;
