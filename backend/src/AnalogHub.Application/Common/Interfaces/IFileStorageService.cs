namespace AnalogHub.Application.Common.Interfaces;

/// <summary>Abstraction over S3-compatible object storage (Cloudflare R2 / AWS S3).</summary>
public interface IFileStorageService
{
    /// <summary>Creates a presigned PUT URL the client uploads the original file to directly.</summary>
    Task<PresignedUploadResult> CreatePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        TimeSpan expiry,
        CancellationToken cancellationToken);

    Task<string> GetPresignedDownloadUrlAsync(
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken);

    /// <summary>Server-side download, used by background processing (thumbnailing, BlurHash) rather than clients.</summary>
    Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>Server-side upload of a generated derivative (thumbnail/preview).</summary>
    Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed record PresignedUploadResult(string UploadUrl, string ObjectKey, DateTimeOffset ExpiresAtUtc);
