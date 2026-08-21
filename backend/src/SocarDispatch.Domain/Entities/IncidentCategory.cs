namespace SocarDispatch.Domain.Entities;

public class IncidentCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;       // "Fire", "Medical", "Security" vb.
    public string Name { get; set; } = string.Empty;       // "Fire Emergency", "Medical Response" vb.
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
