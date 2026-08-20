namespace SocarDispatch.Application.Features.EmergencyCodes.DTOs;

public class EmergencyCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SeverityLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
