namespace AnalogHub.Infrastructure.Storage;

/// <summary>Bound from configuration section "Storage". Works for both AWS S3 and Cloudflare R2 (S3-compatible).</summary>
public sealed class S3StorageOptions
{
    public const string SectionName = "Storage";

    public string BucketName { get; set; } = default!;
    public string Region { get; set; } = "auto";

    /// <summary>Custom endpoint for S3-compatible providers (e.g. Cloudflare R2). Null for real AWS S3.</summary>
    public string? ServiceUrl { get; set; }

    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;

    /// <summary>Public/CDN base URL used to serve derivatives without presigning (e.g. R2 custom domain).</summary>
    public string? PublicBaseUrl { get; set; }
}
