using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Application.Features.Incidents.DTOs;

public class CreateIncidentMediaRequestDto
{
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; } = MediaType.Photo;
}
