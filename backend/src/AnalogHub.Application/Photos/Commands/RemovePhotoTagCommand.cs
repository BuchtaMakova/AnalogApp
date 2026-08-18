using AnalogHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands;

public sealed record RemovePhotoTagCommand(Guid PhotoId, Guid TagId) : IRequest;

public sealed class RemovePhotoTagCommandHandler : IRequestHandler<RemovePhotoTagCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemovePhotoTagCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RemovePhotoTagCommand request, CancellationToken cancellationToken)
    {
        var link = await _db.PhotoTags
            .FirstOrDefaultAsync(
                pt => pt.PhotoId == request.PhotoId && pt.TagId == request.TagId && pt.Photo.UserId == _currentUser.UserId,
                cancellationToken);

        if (link is not null)
        {
            _db.PhotoTags.Remove(link);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
