
namespace SocarDispatch.Domain.Entities;

public class IncidentReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid ReportedByUserId { get; set; }
    public User ReportedBy { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; } // MinIO / S3 Storage URL
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
}
