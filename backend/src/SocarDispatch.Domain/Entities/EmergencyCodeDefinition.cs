namespace SocarDispatch.Domain.Entities;

public class EmergencyCodeDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;       // "Red", "Yellow", "Orange" vb.
    public string ColorHex { get; set; } = string.Empty;   // "#FF3B30", "#FFCC00" vb.
    public string Description { get; set; } = string.Empty;
    public int SeverityLevel { get; set; }                  // 1 (En Yüksek) - 5 (En Düşük)
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
