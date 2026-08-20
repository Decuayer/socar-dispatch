using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Application.Features.Incidents.DTOs;

public class IncidentMediaDto
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public DateTime CreatedAt { get; set; }
}
