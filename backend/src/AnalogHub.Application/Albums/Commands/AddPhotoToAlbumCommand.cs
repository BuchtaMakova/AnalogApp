using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Albums.Commands;

public sealed record AddPhotoToAlbumCommand(Guid AlbumId, Guid PhotoId) : IRequest;

public sealed class AddPhotoToAlbumCommandHandler : IRequestHandler<AddPhotoToAlbumCommand>
{
    private readonly IApplicationDbContext _db;

    public AddPhotoToAlbumCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(AddPhotoToAlbumCommand request, CancellationToken cancellationToken)
    {
        var albumExists = await _db.Albums.AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        var photoExists = await _db.Photos.AnyAsync(p => p.Id == request.PhotoId, cancellationToken);
        if (!photoExists)
        {
            throw new NotFoundException(nameof(Domain.Entities.Photo), request.PhotoId);
        }

        var alreadyLinked = await _db.AlbumPhotos
            .AnyAsync(ap => ap.AlbumId == request.AlbumId && ap.PhotoId == request.PhotoId, cancellationToken);
        if (alreadyLinked)
        {
            return;
        }

        var sortOrder = await _db.AlbumPhotos
            .Where(ap => ap.AlbumId == request.AlbumId)
            .Select(ap => (int?)ap.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        _db.AlbumPhotos.Add(new AlbumPhoto { AlbumId = request.AlbumId, PhotoId = request.PhotoId, SortOrder = sortOrder + 1 });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
