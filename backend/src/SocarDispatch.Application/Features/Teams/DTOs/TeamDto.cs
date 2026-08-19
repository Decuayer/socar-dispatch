namespace SocarDispatch.Application.Features.Teams.DTOs;

public class TeamDto
{
    public Guid Id { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Idle, Forwarded, OnScene, Busy
    public Guid? LeaderId { get; set; }
    public string? LeaderFullName { get; set; }
    public decimal? CurrentLatitude { get; set; }
    public decimal? CurrentLongitude { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TeamMemberDto> Members { get; set; } = new();
}
