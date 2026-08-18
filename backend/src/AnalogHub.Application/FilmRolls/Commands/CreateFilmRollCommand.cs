using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.FilmRolls.Dtos;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.FilmRolls.Commands;

public sealed record CreateFilmRollCommand(
    string Name,
    string Brand,
    FilmFormat Format,
    int NominalIso,
    int? ExposedAtIso,
    int FrameCount,
    Guid? CameraBodyId,
    DateOnly? DateLoaded,
    string? Notes) : IRequest<FilmRollDto>;

public sealed class CreateFilmRollCommandValidator : AbstractValidator<CreateFilmRollCommand>
{
    public CreateFilmRollCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Format).IsInEnum().NotEqual(FilmFormat.Unknown);
        RuleFor(x => x.NominalIso).GreaterThan(0);
        RuleFor(x => x.ExposedAtIso).GreaterThan(0).When(x => x.ExposedAtIso.HasValue);
        RuleFor(x => x.FrameCount).InclusiveBetween(1, 200);
    }
}

public sealed class CreateFilmRollCommandHandler : IRequestHandler<CreateFilmRollCommand, FilmRollDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateFilmRollCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<FilmRollDto> Handle(CreateFilmRollCommand request, CancellationToken cancellationToken)
    {
        CameraBody? cameraBody = null;
        if (request.CameraBodyId is { } cameraBodyId)
        {
            cameraBody = await _db.CameraBodies
                    .FirstOrDefaultAsync(c => c.Id == cameraBodyId && c.UserId == _currentUser.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(CameraBody), cameraBodyId);
        }

        var filmRoll = new FilmRoll
        {
            UserId = _currentUser.UserId,
            Name = request.Name,
            Brand = request.Brand,
            Format = request.Format,
            NominalIso = request.NominalIso,
            ExposedAtIso = request.ExposedAtIso,
            FrameCount = request.FrameCount,
            CameraBodyId = request.CameraBodyId,
            DateLoaded = request.DateLoaded,
            Notes = request.Notes,
            Status = FilmRollStatus.Loaded
        };

        _db.FilmRolls.Add(filmRoll);
        await _db.SaveChangesAsync(cancellationToken);

        return FilmRollMappings.ToDto(filmRoll, cameraBody);
    }
}
