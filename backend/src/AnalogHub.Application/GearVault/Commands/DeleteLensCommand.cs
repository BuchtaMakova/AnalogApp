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

    public DeleteLensCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteLensCommand request, CancellationToken cancellationToken)
    {
        var lens = await _db.Lenses.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Lens), request.Id);

        _db.Lenses.Remove(lens);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
