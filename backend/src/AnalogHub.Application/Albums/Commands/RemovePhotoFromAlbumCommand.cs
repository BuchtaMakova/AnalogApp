using AnalogHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Albums.Commands;

public sealed record RemovePhotoFromAlbumCommand(Guid AlbumId, Guid PhotoId) : IRequest;

public sealed class RemovePhotoFromAlbumCommandHandler : IRequestHandler<RemovePhotoFromAlbumCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemovePhotoFromAlbumCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RemovePhotoFromAlbumCommand request, CancellationToken cancellationToken)
    {
        var link = await _db.AlbumPhotos
            .FirstOrDefaultAsync(
                ap => ap.AlbumId == request.AlbumId && ap.PhotoId == request.PhotoId && ap.Album.UserId == _currentUser.UserId,
                cancellationToken);

        if (link is not null)
        {
            _db.AlbumPhotos.Remove(link);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
