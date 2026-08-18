using System.IO.Compression;
using AnalogHub.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Queries;

/// <summary>Bundles the originals of the selected photos into a single ZIP for bulk download.</summary>
public sealed record DownloadPhotosAsZipQuery(IReadOnlyList<Guid> PhotoIds) : IRequest<DownloadZipResult>;

public sealed record DownloadZipResult(byte[] Content, string FileName);

public sealed class DownloadPhotosAsZipQueryValidator : AbstractValidator<DownloadPhotosAsZipQuery>
{
    public DownloadPhotosAsZipQueryValidator()
    {
        RuleFor(x => x.PhotoIds).NotEmpty();
    }
}

public sealed class DownloadPhotosAsZipQueryHandler : IRequestHandler<DownloadPhotosAsZipQuery, DownloadZipResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public DownloadPhotosAsZipQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<DownloadZipResult> Handle(DownloadPhotosAsZipQuery request, CancellationToken cancellationToken)
    {
        var photos = await _db.Photos.AsNoTracking()
            .Include(p => p.FilmRoll)
            .Where(p => p.UserId == _currentUser.UserId && request.PhotoIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var photo in photos)
            {
                var fileName = PhotoFileNaming.BuildDownloadFileName(photo, usedNames);
                using var content = await _fileStorage.DownloadAsync(photo.OriginalStorageKey, cancellationToken);
                var entry = archive.CreateEntry(fileName, CompressionLevel.NoCompression);
                using var entryStream = entry.Open();
                await content.CopyToAsync(entryStream, cancellationToken);
            }
        }

        return new DownloadZipResult(zipStream.ToArray(), "analoghub-photos.zip");
    }
}
