using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities.Gear;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record DeleteFlashCommand(Guid Id) : IRequest;

public sealed class DeleteFlashCommandHandler : IRequestHandler<DeleteFlashCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteFlashCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteFlashCommand request, CancellationToken cancellationToken)
    {
        var flash = await _db.Flashes.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flash), request.Id);

        _db.Flashes.Remove(flash);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
