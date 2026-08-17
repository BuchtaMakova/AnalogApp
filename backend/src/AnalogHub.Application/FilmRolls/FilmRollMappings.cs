using AnalogHub.Application.FilmRolls.Dtos;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;

namespace AnalogHub.Application.FilmRolls;

internal static class FilmRollMappings
{
    public static FilmRollDto ToDto(FilmRoll r, CameraBody? cameraBody = null) => new(
        r.Id, r.Name, r.Brand, r.Format, r.NominalIso, r.ExposedAtIso, r.FrameCount, r.Status,
        r.CameraBodyId, cameraBody?.Name ?? r.CameraBody?.Name,
        r.DateLoaded, r.DateFinished, r.DateDeveloped, r.LabName, r.DeveloperNotes, r.Notes);
}
