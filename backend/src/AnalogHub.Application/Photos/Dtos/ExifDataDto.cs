namespace AnalogHub.Application.Photos.Dtos;

public sealed record ExifDataDto(
    string? Aperture,
    string? ShutterSpeed,
    int? IsoUsed,
    decimal? FocalLengthMm,
    bool? FlashFired,
    string? MeteringMode,
    string? ScannerModel,
    double? GpsLatitude,
    double? GpsLongitude);
