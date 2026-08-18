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

    /// <param name="forBrowser">
    /// True (default) for URLs returned to the client, which may need a different, publicly
    /// reachable host than the one the API itself uses internally (see
    /// <c>S3StorageOptions.PublicServiceUrl</c>). Pass false when the API server fetches the URL
    /// itself (e.g. downloading a photo to send to a vision model) — that fetch happens on the
    /// API's own network, not the browser's.
    /// </param>
    /// <param name="downloadFileName">
    /// When set, the presigned URL carries a <c>Content-Disposition: attachment</c> response
    /// header override with this file name, so a plain browser navigation (an anchor click, no
    /// JS fetch/CORS involved) saves the file under a friendly name instead of rendering it.
    /// </param>
    Task<string> GetPresignedDownloadUrlAsync(
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken,
        bool forBrowser = true,
        string? downloadFileName = null);

    /// <summary>Server-side download, used by background processing (thumbnailing, BlurHash) rather than clients.</summary>
    Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>Server-side upload of a generated derivative (thumbnail/preview).</summary>
    Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed record PresignedUploadResult(string UploadUrl, string ObjectKey, DateTimeOffset ExpiresAtUtc);
