using System.Text.Json;
using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Dtos;
using AnalogHub.Application.Photos;
using AnalogHub.Application.Tags.Dtos;
using AnalogHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Queries;

public sealed record GetPhotoByIdQuery(Guid Id) : IRequest<PhotoDetailDto>;

public sealed class GetPhotoByIdQueryHandler : IRequestHandler<GetPhotoByIdQuery, PhotoDetailDto>
{
    private static readonly TimeSpan UrlExpiry = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public GetPhotoByIdQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<PhotoDetailDto> Handle(GetPhotoByIdQuery request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos
            .AsNoTracking()
            .Include(p => p.FilmRoll)
            .Include(p => p.CameraBody)
            .Include(p => p.Lens)
            .Include(p => p.Flash)
            .Include(p => p.Critique)
            .Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.AlbumPhotos)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.Id);

        var originalUrl = await _fileStorage.GetPresignedDownloadUrlAsync(photo.OriginalStorageKey, UrlExpiry, cancellationToken);
        var previewUrl = photo.PreviewStorageKey is null
            ? null
            : await _fileStorage.GetPresignedDownloadUrlAsync(photo.PreviewStorageKey, UrlExpiry, cancellationToken);
        var thumbnailUrl = photo.ThumbnailStorageKey is null
            ? null
            : await _fileStorage.GetPresignedDownloadUrlAsync(photo.ThumbnailStorageKey, UrlExpiry, cancellationToken);
        var downloadFileName = PhotoFileNaming.BuildDownloadFileName(photo);
        var downloadUrl = await _fileStorage.GetPresignedDownloadUrlAsync(
            photo.OriginalStorageKey, UrlExpiry, cancellationToken, downloadFileName: downloadFileName);

        PhotoCritiqueDto? critiqueDto = null;
        if (photo.Critique is { } critique)
        {
            var recommendations = JsonSerializer.Deserialize<List<string>>(critique.RecommendationsJson) ?? [];
            var suggestedTags = JsonSerializer.Deserialize<List<string>>(critique.SuggestedTagsJson) ?? [];

            critiqueDto = new PhotoCritiqueDto(
                photo.Id, critique.Model, critique.CompositionScore ?? 0, critique.CompositionNotes ?? "",
                critique.LightingNotes ?? "", critique.PosingNotes, recommendations, suggestedTags,
                critique.CreatedAtUtc);
        }

        var exif = new ExifDataDto(
            photo.Exif.Aperture, photo.Exif.ShutterSpeed, photo.Exif.IsoUsed, photo.Exif.FocalLengthMm,
            photo.Exif.FlashFired, photo.Exif.MeteringMode, photo.Exif.ScannerModel,
            photo.Exif.GpsLatitude, photo.Exif.GpsLongitude);

        var tags = photo.PhotoTags
            .Select(pt => new PhotoTagDto(pt.TagId, pt.Tag.Name, pt.Tag.Slug, pt.IsAiSuggested))
            .OrderBy(t => t.Name)
            .ToList();

        return new PhotoDetailDto(
            photo.Id,
            photo.FilmRollId,
            photo.FilmRoll.Name,
            photo.CameraBodyId,
            photo.CameraBody is null ? null : $"{photo.CameraBody.Brand} {photo.CameraBody.Model}",
            photo.LensId,
            photo.Lens is null ? null : $"{photo.Lens.Brand} {photo.Lens.Model}",
            photo.FlashId,
            photo.Flash is null ? null : $"{photo.Flash.Brand} {photo.Flash.Model}",
            photo.FrameNumber,
            photo.CaptureDateUtc,
            originalUrl,
            previewUrl,
            thumbnailUrl,
            downloadUrl,
            photo.BlurHash,
            photo.WidthPx,
            photo.HeightPx,
            photo.FileSizeBytes,
            photo.ContentType,
            photo.Rating,
            photo.RotationDegrees,
            photo.ProcessingStatus,
            exif,
            critiqueDto,
            tags,
            photo.AlbumPhotos.Select(ap => ap.AlbumId).ToList(),
            photo.CreatedAtUtc);
    }
}
