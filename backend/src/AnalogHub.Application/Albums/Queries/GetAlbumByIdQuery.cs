using AnalogHub.Application.Albums.Dtos;
using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Dtos;
using AnalogHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Albums.Queries;

public sealed record GetAlbumByIdQuery(Guid Id) : IRequest<AlbumDetailDto>;

public sealed class GetAlbumByIdQueryHandler : IRequestHandler<GetAlbumByIdQuery, AlbumDetailDto>
{
    private static readonly TimeSpan ThumbnailUrlExpiry = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public GetAlbumByIdQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<AlbumDetailDto> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        var album = await _db.Albums.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Album), request.Id);

        var photos = await _db.AlbumPhotos
            .AsNoTracking()
            .Where(ap => ap.AlbumId == request.Id)
            .OrderBy(ap => ap.SortOrder)
            .Select(ap => new
            {
                ap.Photo.Id,
                ap.Photo.FilmRollId,
                ap.Photo.ThumbnailStorageKey,
                ap.Photo.BlurHash,
                ap.Photo.WidthPx,
                ap.Photo.HeightPx,
                ap.Photo.Rating,
                ap.Photo.ProcessingStatus,
                ap.Photo.CaptureDateUtc,
                Tags = ap.Photo.PhotoTags.Select(pt => pt.Tag.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        var photoDtos = new List<PhotoListItemDto>(photos.Count);
        foreach (var photo in photos)
        {
            var thumbnailUrl = photo.ThumbnailStorageKey is null
                ? null
                : await _fileStorage.GetPresignedDownloadUrlAsync(photo.ThumbnailStorageKey, ThumbnailUrlExpiry, cancellationToken);

            photoDtos.Add(new PhotoListItemDto(
                photo.Id, photo.FilmRollId, thumbnailUrl, photo.BlurHash, photo.WidthPx, photo.HeightPx,
                photo.Rating, photo.ProcessingStatus, photo.CaptureDateUtc, photo.Tags));
        }

        return new AlbumDetailDto(album.Id, album.Name, album.Description, album.CoverPhotoId, album.CreatedAtUtc, photoDtos);
    }
}
