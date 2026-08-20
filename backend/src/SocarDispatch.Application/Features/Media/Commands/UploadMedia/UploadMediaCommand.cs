using MediatR;
using Microsoft.AspNetCore.Http;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Media.DTOs;

namespace SocarDispatch.Application.Features.Media.Commands.UploadMedia;

public record UploadMediaCommand(IFormFile File, string Category = "incident")
    : IRequest<ApiResponse<MediaUploadResponseDto>>;
