namespace AnalogHub.Infrastructure.Storage;

/// <summary>Bound from configuration section "Storage". Works for both AWS S3 and Cloudflare R2 (S3-compatible).</summary>
public sealed class S3StorageOptions
{
    public const string SectionName = "Storage";

    public string BucketName { get; set; } = default!;
    public string Region { get; set; } = "auto";

    /// <summary>Custom endpoint for S3-compatible providers (e.g. Cloudflare R2). Null for real AWS S3.</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Browser-facing origin substituted into presigned URLs when <see cref="ServiceUrl"/> itself
    /// isn't reachable from outside the API's own network (e.g. ServiceUrl is the Docker Compose
    /// service name <c>http://minio:9000</c>, which only resolves inside the compose network, while
    /// the browser needs the host-published <c>http://localhost:9000</c>). Leave unset when
    /// ServiceUrl is already reachable from the browser (real AWS S3, Cloudflare R2, or native dev
    /// against a host-published MinIO) — presigned URLs are then left as the SDK signed them.
    /// </summary>
    public string? PublicServiceUrl { get; set; }

    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;

    /// <summary>
    /// Cloudflare R2 and real AWS S3 only accept SigV4-signed requests; MinIO accepts both and this
    /// defaults to false (SigV2) to match the existing local-dev setup, where <see cref="ServiceUrl"/>
    /// deliberately differs between the API container and the browser (see <see cref="PublicServiceUrl"/>)
    /// — SigV2's string-to-sign doesn't cover host/scheme, so that rewrite stays valid; SigV4's does,
    /// so rewriting after signing would invalidate the signature. Set true for R2/AWS S3, where
    /// ServiceUrl is already the one URL both sides use, so no rewrite happens regardless.
    /// </summary>
    public bool UseSignatureVersion4 { get; set; }

    /// <summary>Public/CDN base URL used to serve derivatives without presigning (e.g. R2 custom domain).</summary>
    public string? PublicBaseUrl { get; set; }
}
