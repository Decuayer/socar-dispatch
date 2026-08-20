
using MediatR;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Media.DTOs;

namespace SocarDispatch.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, ApiResponse<MediaUploadResponseDto>>
{
    private readonly IMediaStorageService _mediaStorageService;

    public UploadMediaCommandHandler(IMediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    public async Task<ApiResponse<MediaUploadResponseDto>> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        using var stream = request.File.OpenReadStream();

        var result = await _mediaStorageService.UploadAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.Category,
            cancellationToken);

        var dto = new MediaUploadResponseDto
        {
            ObjectKey = result.ObjectKey,
            MediaUrl = result.PublicUrl,
            FileSizeBytes = result.FileSizeBytes,
            ContentType = request.File.ContentType
        };

        return ApiResponse<MediaUploadResponseDto>.SuccessResult(dto, "The media file was successfully uploaded.");
    }
}
