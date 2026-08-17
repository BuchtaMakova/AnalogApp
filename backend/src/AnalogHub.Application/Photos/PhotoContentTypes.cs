namespace AnalogHub.Application.Photos;

/// <summary>MIME types accepted for scanned frame uploads.</summary>
public static class PhotoContentTypes
{
    public static readonly IReadOnlySet<string> Allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/tiff",
        "image/webp"
    };

    /// <summary>Scans are large; cap well above a typical high-res flatbed/lab scan TIFF.</summary>
    public const long MaxFileSizeBytes = 200 * 1024 * 1024;
}
