using AnalogHub.Application.Albums.Dtos;
using AnalogHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Albums.Queries;

public sealed record GetAlbumsQuery : IRequest<IReadOnlyList<AlbumDto>>;

public sealed class GetAlbumsQueryHandler : IRequestHandler<GetAlbumsQuery, IReadOnlyList<AlbumDto>>
{
    private static readonly TimeSpan CoverUrlExpiry = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public GetAlbumsQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AlbumDto>> Handle(GetAlbumsQuery request, CancellationToken cancellationToken)
    {
        var albums = await _db.Albums
            .AsNoTracking()
            .Where(a => a.UserId == _currentUser.UserId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Description,
                a.CoverPhotoId,
                CoverThumbnailKey = a.CoverPhoto == null ? null : a.CoverPhoto.ThumbnailStorageKey,
                PhotoCount = a.AlbumPhotos.Count,
                a.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var result = new List<AlbumDto>(albums.Count);
        foreach (var album in albums)
        {
            var coverUrl = album.CoverThumbnailKey is null
                ? null
                : await _fileStorage.GetPresignedDownloadUrlAsync(album.CoverThumbnailKey, CoverUrlExpiry, cancellationToken);

            result.Add(new AlbumDto(album.Id, album.Name, album.Description, album.CoverPhotoId, coverUrl, album.PhotoCount, album.CreatedAtUtc));
        }

        return result;
    }
}
