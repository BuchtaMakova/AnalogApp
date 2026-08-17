using AnalogHub.Application.Photos.Dtos;

namespace AnalogHub.Application.Albums.Dtos;

public sealed record AlbumDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? CoverPhotoId,
    string? CoverPhotoThumbnailUrl,
    int PhotoCount,
    DateTimeOffset CreatedAtUtc);

public sealed record AlbumDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? CoverPhotoId,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<PhotoListItemDto> Photos);
