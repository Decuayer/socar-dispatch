using Minio;
using Minio.DataModel.Args;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Settings;

namespace SocarDispatch.Infrastructure.Services;

public class MinioStorageService : IMediaStorageService
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "video/mp4"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB (52,428,800 bytes)

    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;

    public MinioStorageService(MinioSettings settings)
    {
        _settings = settings;

        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSSL)
            .Build();
    }

    public async Task<MediaUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string category,
        CancellationToken ct = default)
    {
        // 1. MIME Type Validation
        if (!AllowedMimeTypes.Contains(contentType))
        {
            throw new DomainException(
                $"Desteklenmeyen dosya tipi: '{contentType}'. Yalnızca JPEG, PNG ve MP4 formatları kabul edilmektedir.");
        }

        // 2. File Size Validation (50MB)
        if (fileStream.Length > MaxFileSizeBytes)
        {
            throw new DomainException("Dosya boyutu 50MB limitini aşıyor.");
        }

        // 3. Bucket Existence Check
        await EnsureBucketExistsAsync(ct);

        // 4. Object Key Formatting: {category}/yyyy/MM/dd/{Guid}{extension}
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "video/mp4" => ".mp4",
                _ => ""
            };
        }

        var sanitizedCategory = string.IsNullOrWhiteSpace(category) ? "general" : category.Trim().ToLowerInvariant();
        var objectKey = $"{sanitizedCategory}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}{extension}";

        // 5. Upload to MinIO
        fileStream.Position = 0;
        var putArgs = new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectKey)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putArgs, ct);

        // 6. Form Public Access URL
        var publicUrl = $"{_settings.PublicEndpoint.TrimEnd('/')}/{_settings.BucketName}/{objectKey}";

        return new MediaUploadResult(objectKey, publicUrl, fileStream.Length);
    }

    public async Task<string> GetPreSignedUrlAsync(string objectKey, TimeSpan expiry, CancellationToken ct = default)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectKey)
            .WithExpiry((int)expiry.TotalSeconds);

        return await _minioClient.PresignedGetObjectAsync(presignedArgs);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectKey);

        await _minioClient.RemoveObjectAsync(removeArgs, ct);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
        bool found = await _minioClient.BucketExistsAsync(existsArgs, ct);
        if (!found)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
            await _minioClient.MakeBucketAsync(makeArgs, ct);
        }
    }
}
