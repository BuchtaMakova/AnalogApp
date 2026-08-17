using AnalogHub.Domain.Enums;

namespace AnalogHub.Application.FilmRolls.Dtos;

public sealed record FilmRollDto(
    Guid Id,
    string Name,
    string Brand,
    FilmFormat Format,
    int NominalIso,
    int? ExposedAtIso,
    int FrameCount,
    FilmRollStatus Status,
    Guid? CameraBodyId,
    string? CameraBodyName,
    DateOnly? DateLoaded,
    DateOnly? DateFinished,
    DateOnly? DateDeveloped,
    string? LabName,
    string? DeveloperNotes,
    string? Notes);
