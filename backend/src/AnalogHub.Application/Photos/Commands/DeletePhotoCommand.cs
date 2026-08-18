using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands;

public sealed record DeletePhotoCommand(Guid Id) : IRequest;

public sealed class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public DeletePhotoCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.Id);

        // Storage cleanup happens before the DB delete: if this fails, the row (and the chance to
        // retry) stays around instead of the DB record vanishing while orphaned objects remain in
        // the bucket forever.
        foreach (var storageKey in new[] { photo.OriginalStorageKey, photo.PreviewStorageKey, photo.ThumbnailStorageKey })
        {
            if (storageKey is not null)
            {
                await _fileStorage.DeleteAsync(storageKey, cancellationToken);
            }
        }

        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
