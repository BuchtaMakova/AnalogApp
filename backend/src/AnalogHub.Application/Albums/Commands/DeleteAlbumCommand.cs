using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Albums.Commands;

public sealed record DeleteAlbumCommand(Guid Id) : IRequest;

public sealed class DeleteAlbumCommandHandler : IRequestHandler<DeleteAlbumCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteAlbumCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Album), request.Id);

        _db.Albums.Remove(album);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
