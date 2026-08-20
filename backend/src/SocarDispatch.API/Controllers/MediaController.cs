using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Media.Commands.UploadMedia;
using SocarDispatch.Application.Features.Media.DTOs;

namespace SocarDispatch.API.Controllers;

[ApiController]
[Route("api/v1/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly ISender _sender;

    public MediaController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// POST /api/v1/media/upload
    /// Uploads an image (JPEG, PNG) or video (MP4) file up to 50MB to MinIO object storage.
    /// </summary>
    /// <param name="request">File payload and category (multipart/form-data)</param>
    /// <returns>Public URL and object key details</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<MediaUploadResponseDto>>> Upload(
        [FromForm] UploadMediaRequestDto request)
    {
        var command = new UploadMediaCommand(request.File, request.Category);
        var result = await _sender.Send(command);
        return Ok(result);
    }
}
