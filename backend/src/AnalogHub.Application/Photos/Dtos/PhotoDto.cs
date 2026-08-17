using AnalogHub.Domain.Enums;

namespace AnalogHub.Application.Photos.Dtos;

public sealed record PhotoDto(
    Guid Id,
    Guid FilmRollId,
    Guid? CameraBodyId,
    Guid? LensId,
    Guid? FlashId,
    int? FrameNumber,
    DateTimeOffset? CaptureDateUtc,
    string OriginalStorageKey,
    string? PreviewStorageKey,
    string? ThumbnailStorageKey,
    string? BlurHash,
    int? WidthPx,
    int? HeightPx,
    long FileSizeBytes,
    string ContentType,
    byte Rating,
    PhotoProcessingStatus ProcessingStatus,
    ExifDataDto Exif,
    DateTimeOffset CreatedAtUtc);
