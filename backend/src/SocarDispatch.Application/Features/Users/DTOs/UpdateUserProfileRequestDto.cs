namespace SocarDispatch.Application.Features.Users.DTOs;

public class UpdateUserProfileRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? SubRole { get; set; }
    public string? AvatarUrl { get; set; }
}
