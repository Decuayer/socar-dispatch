using Microsoft.AspNetCore.Http;

namespace SocarDispatch.Application.Features.Media.DTOs;

public class UploadMediaRequestDto
{
    public required IFormFile File { get; set; }
    public string Category { get; set; } = "incident";
}
