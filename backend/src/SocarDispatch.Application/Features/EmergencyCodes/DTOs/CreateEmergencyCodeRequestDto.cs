namespace SocarDispatch.Application.Features.EmergencyCodes.DTOs;

public class CreateEmergencyCodeRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SeverityLevel { get; set; }
}
