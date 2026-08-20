using SocarDispatch.Application.Common.Models;

namespace SocarDispatch.Application.Common.Interfaces;

public interface IMediaStorageService
{
    Task<MediaUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string category,
        CancellationToken ct = default);

    Task<string> GetPreSignedUrlAsync(string objectKey, TimeSpan expiry, CancellationToken ct = default);
    Task DeleteAsync(string objectKey, CancellationToken ct = default);
}
