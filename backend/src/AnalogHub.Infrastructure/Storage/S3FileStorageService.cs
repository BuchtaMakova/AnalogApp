using AnalogHub.Application.Common.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace AnalogHub.Infrastructure.Storage;

/// <summary>IFileStorageService backed by the AWS S3 SDK, pointed at Cloudflare R2 or AWS S3 via ServiceUrl.</summary>
public sealed class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3StorageOptions _options;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<S3StorageOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public Task<PresignedUploadResult> CreatePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        var expiresAtUtc = DateTime.UtcNow.Add(expiry);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = expiresAtUtc
        };

        var url = FixScheme(_s3Client.GetPreSignedURL(request));
        url = RewriteOrigin(url, _options.PublicServiceUrl);

        return Task.FromResult(new PresignedUploadResult(url, objectKey, expiresAtUtc));
    }

    public Task<string> GetPresignedDownloadUrlAsync(
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken,
        bool forBrowser = true,
        string? downloadFileName = null)
    {
        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return Task.FromResult($"{_options.PublicBaseUrl.TrimEnd('/')}/{objectKey}");
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        if (downloadFileName is not null)
        {
            request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename=\"{downloadFileName}\"";
        }

        var url = FixScheme(_s3Client.GetPreSignedURL(request));
        if (forBrowser)
        {
            url = RewriteOrigin(url, _options.PublicServiceUrl);
        }

        return Task.FromResult(url);
    }

    /// <summary>
    /// The SDK signs presigned URLs with SigV2 for custom (non-AWS) endpoints, whose string-to-sign
    /// doesn't cover scheme or host — but it still hands back an https:// URL even when
    /// <see cref="S3StorageOptions.ServiceUrl"/> is http:// (e.g. local MinIO without TLS). Applied
    /// unconditionally: every consumer of a presigned URL, browser or the API server itself, needs
    /// this fixed, since a bare scheme mismatch fails the TCP/TLS handshake outright.
    /// </summary>
    private string FixScheme(string presignedUrl)
    {
        if (_options.ServiceUrl is not { Length: > 0 } serviceUrl || !Uri.TryCreate(serviceUrl, UriKind.Absolute, out var configured))
        {
            return presignedUrl;
        }

        return presignedUrl.Replace($"{Uri.UriSchemeHttps}://", $"{configured.Scheme}://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Swaps the presigned URL's origin (scheme+host+port) to <paramref name="targetBaseUrl"/> when
    /// set — used to hand browser-facing URLs a host they can actually reach, when
    /// <see cref="S3StorageOptions.ServiceUrl"/> is only reachable from the API's own network (e.g.
    /// the Docker Compose service name <c>http://minio:9000</c>). Only the origin changes — path and
    /// query (including the signature) are copied through untouched, safe for the same reason
    /// <see cref="FixScheme"/> is: SigV2's string-to-sign doesn't cover scheme or host.
    /// </summary>
    private static string RewriteOrigin(string presignedUrl, string? targetBaseUrl)
    {
        if (targetBaseUrl is not { Length: > 0 } || !Uri.TryCreate(targetBaseUrl, UriKind.Absolute, out var target))
        {
            return presignedUrl;
        }

        if (!Uri.TryCreate(presignedUrl, UriKind.Absolute, out var presigned))
        {
            return presignedUrl;
        }

        var targetOrigin = $"{target.Scheme}://{target.Authority}";
        var presignedOrigin = $"{presigned.Scheme}://{presigned.Authority}";
        return targetOrigin + presignedUrl[presignedOrigin.Length..];
    }

    public async Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken)
    {
        var response = await _s3Client.GetObjectAsync(
            new GetObjectRequest { BucketName = _options.BucketName, Key = objectKey },
            cancellationToken);

        var buffer = new MemoryStream();
        await response.ResponseStream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        return buffer;
    }

    public async Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        await _s3Client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey,
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false,
                // The SDK's default chunked SigV4 upload signing (a rolling signature per chunk as
                // the request streams) is an AWS-specific extension real S3 supports but R2 doesn't —
                // PutObject fails with "STREAMING-AWS4-HMAC-SHA256-PAYLOAD not implemented". This
                // makes the SDK hash the whole payload upfront and sign once instead; harmless for
                // MinIO too, and irrelevant at photo-derivative sizes.
                UseChunkEncoding = false
            },
            cancellationToken);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        await _s3Client.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = _options.BucketName, Key = objectKey },
            cancellationToken);
    }
}
