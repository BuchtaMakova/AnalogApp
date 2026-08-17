namespace AnalogHub.Domain.ValueObjects;

/// <summary>
/// EXIF/metadata captured for a photo, either read from the scanned file or entered manually
/// (analog scans rarely carry real capture-time EXIF, so most fields are nullable).
/// Persisted as an EF Core owned type on <see cref="Entities.Photo"/>.
/// </summary>
public sealed class ExifData
{
    public string? Aperture { get; set; }
    public string? ShutterSpeed { get; set; }
    public int? IsoUsed { get; set; }
    public decimal? FocalLengthMm { get; set; }
    public bool? FlashFired { get; set; }
    public string? MeteringMode { get; set; }
    public string? ScannerModel { get; set; }
    public double? GpsLatitude { get; set; }
    public double? GpsLongitude { get; set; }
}
