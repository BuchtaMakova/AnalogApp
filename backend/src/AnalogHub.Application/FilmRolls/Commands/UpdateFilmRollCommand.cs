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

public sealed record UpdateFilmRollCommand(
    Guid Id,
    string Name,
    string Brand,
    FilmFormat Format,
    int NominalIso,
    int? ExposedAtIso,
    int FrameCount,
    FilmRollStatus Status,
    Guid? CameraBodyId,
    DateOnly? DateLoaded,
    DateOnly? DateFinished,
    DateOnly? DateDeveloped,
    string? LabName,
    string? DeveloperNotes,
    string? Notes) : IRequest<FilmRollDto>;

public sealed class UpdateFilmRollCommandValidator : AbstractValidator<UpdateFilmRollCommand>
{
    public UpdateFilmRollCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Format).IsInEnum().NotEqual(FilmFormat.Unknown);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.NominalIso).GreaterThan(0);
        RuleFor(x => x.ExposedAtIso).GreaterThan(0).When(x => x.ExposedAtIso.HasValue);
        RuleFor(x => x.FrameCount).InclusiveBetween(1, 200);
    }
}

public sealed class UpdateFilmRollCommandHandler : IRequestHandler<UpdateFilmRollCommand, FilmRollDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateFilmRollCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<FilmRollDto> Handle(UpdateFilmRollCommand request, CancellationToken cancellationToken)
    {
        var filmRoll = await _db.FilmRolls.FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(FilmRoll), request.Id);

        CameraBody? cameraBody = null;
        if (request.CameraBodyId is { } cameraBodyId)
        {
            cameraBody = await _db.CameraBodies
                    .FirstOrDefaultAsync(c => c.Id == cameraBodyId && c.UserId == _currentUser.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(CameraBody), cameraBodyId);
        }

        filmRoll.Name = request.Name;
        filmRoll.Brand = request.Brand;
        filmRoll.Format = request.Format;
        filmRoll.NominalIso = request.NominalIso;
        filmRoll.ExposedAtIso = request.ExposedAtIso;
        filmRoll.FrameCount = request.FrameCount;
        filmRoll.Status = request.Status;
        filmRoll.CameraBodyId = request.CameraBodyId;
        filmRoll.DateLoaded = request.DateLoaded;
        filmRoll.DateFinished = request.DateFinished;
        filmRoll.DateDeveloped = request.DateDeveloped;
        filmRoll.LabName = request.LabName;
        filmRoll.DeveloperNotes = request.DeveloperNotes;
        filmRoll.Notes = request.Notes;

        await _db.SaveChangesAsync(cancellationToken);

        return FilmRollMappings.ToDto(filmRoll, cameraBody);
    }
}
