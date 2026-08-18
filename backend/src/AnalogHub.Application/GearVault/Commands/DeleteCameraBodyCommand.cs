using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities.Gear;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record DeleteCameraBodyCommand(Guid Id) : IRequest;

public sealed class DeleteCameraBodyCommandHandler : IRequestHandler<DeleteCameraBodyCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCameraBodyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteCameraBodyCommand request, CancellationToken cancellationToken)
    {
        var cameraBody = await _db.CameraBodies.FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(CameraBody), request.Id);

        _db.CameraBodies.Remove(cameraBody);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
