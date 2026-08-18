using AnalogHub.Application.Tags.Dtos;
using AnalogHub.Domain.Enums;

namespace AnalogHub.Application.Photos.Dtos;

public sealed record PhotoListItemDto(
    Guid Id,
    Guid FilmRollId,
    string? ThumbnailUrl,
    string? BlurHash,
    int? WidthPx,
    int? HeightPx,
    byte Rating,
    int RotationDegrees,
    PhotoProcessingStatus ProcessingStatus,
    DateTimeOffset? CaptureDateUtc,
    IReadOnlyList<string> Tags);

public sealed record PhotoDetailDto(
    Guid Id,
    Guid FilmRollId,
    string FilmRollName,
    Guid? CameraBodyId,
    string? CameraBodyName,
    Guid? LensId,
    string? LensName,
    Guid? FlashId,
    string? FlashName,
    int? FrameNumber,
    DateTimeOffset? CaptureDateUtc,
    string? OriginalUrl,
    string? PreviewUrl,
    string? ThumbnailUrl,
    string DownloadUrl,
    string? BlurHash,
    int? WidthPx,
    int? HeightPx,
    long FileSizeBytes,
    string ContentType,
    byte Rating,
    int RotationDegrees,
    PhotoProcessingStatus ProcessingStatus,
    ExifDataDto Exif,
    PhotoCritiqueDto? Critique,
    IReadOnlyList<PhotoTagDto> Tags,
    IReadOnlyList<Guid> AlbumIds,
    DateTimeOffset CreatedAtUtc);

/// <summary>A presigned URL with a friendly Content-Disposition filename, for "download separately" bulk actions.</summary>
public sealed record PhotoDownloadLinkDto(Guid PhotoId, string Url, string FileName);
