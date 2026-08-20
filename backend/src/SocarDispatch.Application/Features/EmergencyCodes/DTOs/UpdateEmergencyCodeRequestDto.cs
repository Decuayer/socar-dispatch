namespace SocarDispatch.Application.Features.EmergencyCodes.DTOs;

public class UpdateEmergencyCodeRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SeverityLevel { get; set; }
    public bool IsActive { get; set; } = true;
}
