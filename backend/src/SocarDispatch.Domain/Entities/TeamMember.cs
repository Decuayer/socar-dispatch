namespace SocarDispatch.Domain.Entities;

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}