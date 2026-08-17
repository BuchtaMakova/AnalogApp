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

        var url = MatchConfiguredScheme(_s3Client.GetPreSignedURL(request));

        return Task.FromResult(new PresignedUploadResult(url, objectKey, expiresAtUtc));
    }

    public Task<string> GetPresignedDownloadUrlAsync(
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken)
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

        return Task.FromResult(MatchConfiguredScheme(_s3Client.GetPreSignedURL(request)));
    }

    /// <summary>
    /// The SDK signs presigned URLs with SigV2 for custom (non-AWS) endpoints, whose string-to-sign
    /// doesn't cover scheme or host — but it still hands back an https:// URL even when
    /// <see cref="S3StorageOptions.ServiceUrl"/> is http:// (e.g. local MinIO without TLS). Since the
    /// signature doesn't depend on scheme, it's safe to align it with the configured endpoint here.
    /// </summary>
    private string MatchConfiguredScheme(string presignedUrl)
    {
        if (_options.ServiceUrl is not { Length: > 0 } serviceUrl || !Uri.TryCreate(serviceUrl, UriKind.Absolute, out var configured))
        {
            return presignedUrl;
        }

        return presignedUrl.Replace($"{Uri.UriSchemeHttps}://", $"{configured.Scheme}://", StringComparison.OrdinalIgnoreCase);
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
                AutoCloseStream = false
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
