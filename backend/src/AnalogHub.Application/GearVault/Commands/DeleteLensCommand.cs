using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities.Gear;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record DeleteLensCommand(Guid Id) : IRequest;

public sealed class DeleteLensCommandHandler : IRequestHandler<DeleteLensCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteLensCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteLensCommand request, CancellationToken cancellationToken)
    {
        var lens = await _db.Lenses.FirstOrDefaultAsync(l => l.Id == request.Id && l.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Lens), request.Id);

        _db.Lenses.Remove(lens);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
