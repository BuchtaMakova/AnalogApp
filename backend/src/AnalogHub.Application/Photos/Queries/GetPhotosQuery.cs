using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Common.Models;
using AnalogHub.Application.Photos.Dtos;
using AnalogHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Queries;

/// <summary>Paginated, filterable photo listing that backs the library grid's infinite scroll.</summary>
public sealed record GetPhotosQuery(
    Guid? FilmRollId = null,
    Guid? CameraBodyId = null,
    Guid? LensId = null,
    Guid? FlashId = null,
    string? TagSlug = null,
    byte? MinRating = null,
    int Page = 1,
    int PageSize = 60) : IRequest<PagedResult<PhotoListItemDto>>;

public sealed class GetPhotosQueryHandler : IRequestHandler<GetPhotosQuery, PagedResult<PhotoListItemDto>>
{
    private static readonly TimeSpan ThumbnailUrlExpiry = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public GetPhotosQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PhotoListItemDto>> Handle(GetPhotosQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var query = _db.Photos.AsNoTracking()
            .Where(p => p.UserId == _currentUser.UserId && p.ProcessingStatus != PhotoProcessingStatus.PendingUpload);

        if (request.FilmRollId is { } filmRollId) query = query.Where(p => p.FilmRollId == filmRollId);
        if (request.CameraBodyId is { } cameraBodyId) query = query.Where(p => p.CameraBodyId == cameraBodyId);
        if (request.LensId is { } lensId) query = query.Where(p => p.LensId == lensId);
        if (request.FlashId is { } flashId) query = query.Where(p => p.FlashId == flashId);
        if (request.MinRating is { } minRating) query = query.Where(p => p.Rating >= minRating);
        if (!string.IsNullOrWhiteSpace(request.TagSlug))
        {
            query = query.Where(p => p.PhotoTags.Any(pt => pt.Tag.Slug == request.TagSlug));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var photos = await query
            .OrderByDescending(p => p.CaptureDateUtc ?? p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.FilmRollId,
                p.ThumbnailStorageKey,
                p.BlurHash,
                p.WidthPx,
                p.HeightPx,
                p.Rating,
                p.RotationDegrees,
                p.ProcessingStatus,
                p.CaptureDateUtc,
                Tags = p.PhotoTags.Select(pt => pt.Tag.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        var items = new List<PhotoListItemDto>(photos.Count);
        foreach (var photo in photos)
        {
            var thumbnailUrl = photo.ThumbnailStorageKey is null
                ? null
                : await _fileStorage.GetPresignedDownloadUrlAsync(photo.ThumbnailStorageKey, ThumbnailUrlExpiry, cancellationToken);

            items.Add(new PhotoListItemDto(
                photo.Id, photo.FilmRollId, thumbnailUrl, photo.BlurHash, photo.WidthPx, photo.HeightPx,
                photo.Rating, photo.RotationDegrees, photo.ProcessingStatus, photo.CaptureDateUtc, photo.Tags));
        }

        return new PagedResult<PhotoListItemDto>(items, page, pageSize, totalCount);
    }
}
