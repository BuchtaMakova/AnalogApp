using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.FilmRolls.Commands;

public sealed record DeleteFilmRollCommand(Guid Id) : IRequest;

public sealed class DeleteFilmRollCommandHandler : IRequestHandler<DeleteFilmRollCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteFilmRollCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteFilmRollCommand request, CancellationToken cancellationToken)
    {
        var filmRoll = await _db.FilmRolls.FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(FilmRoll), request.Id);

        _db.FilmRolls.Remove(filmRoll);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
