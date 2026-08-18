using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Queries;

/// <summary>
/// Presigned, force-download URLs for a batch of photos — backs the "download separately" bulk
/// action, where the browser triggers one native download per photo without going through the
/// API server's own bandwidth (unlike the ZIP path, which does).
/// </summary>
public sealed record GetPhotoDownloadUrlsQuery(IReadOnlyList<Guid> PhotoIds) : IRequest<IReadOnlyList<PhotoDownloadLinkDto>>;

public sealed class GetPhotoDownloadUrlsQueryValidator : AbstractValidator<GetPhotoDownloadUrlsQuery>
{
    public GetPhotoDownloadUrlsQueryValidator()
    {
        RuleFor(x => x.PhotoIds).NotEmpty();
    }
}

public sealed class GetPhotoDownloadUrlsQueryHandler : IRequestHandler<GetPhotoDownloadUrlsQuery, IReadOnlyList<PhotoDownloadLinkDto>>
{
    private static readonly TimeSpan UrlExpiry = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public GetPhotoDownloadUrlsQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PhotoDownloadLinkDto>> Handle(GetPhotoDownloadUrlsQuery request, CancellationToken cancellationToken)
    {
        var photos = await _db.Photos.AsNoTracking()
            .Include(p => p.FilmRoll)
            .Where(p => p.UserId == _currentUser.UserId && request.PhotoIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var links = new List<PhotoDownloadLinkDto>(photos.Count);
        foreach (var photo in photos)
        {
            var fileName = PhotoFileNaming.BuildDownloadFileName(photo, usedNames);
            var url = await _fileStorage.GetPresignedDownloadUrlAsync(
                photo.OriginalStorageKey, UrlExpiry, cancellationToken, downloadFileName: fileName);
            links.Add(new PhotoDownloadLinkDto(photo.Id, url, fileName));
        }

        return links;
    }
}
