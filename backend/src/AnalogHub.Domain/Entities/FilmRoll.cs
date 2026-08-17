using AnalogHub.Domain.Common;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;

namespace AnalogHub.Domain.Entities;

public sealed class FilmRoll : AuditableEntity
{
    /// <summary>Stock name, e.g. "Kodak Portra 400".</summary>
    public string Name { get; set; } = default!;
    public string Brand { get; set; } = default!;
    public FilmFormat Format { get; set; }
    public int NominalIso { get; set; }

    /// <summary>Actual shooting ISO if the roll was pushed/pulled; null means shot at box speed.</summary>
    public int? ExposedAtIso { get; set; }

    /// <summary>Total frames on the roll (e.g. 24/36 for 35mm, 8-16 for 120 depending on camera format).</summary>
    public int FrameCount { get; set; }

    public FilmRollStatus Status { get; set; } = FilmRollStatus.Loaded;

    public Guid? CameraBodyId { get; set; }
    public CameraBody? CameraBody { get; set; }

    public DateOnly? DateLoaded { get; set; }
    public DateOnly? DateFinished { get; set; }
    public DateOnly? DateDeveloped { get; set; }

    public string? LabName { get; set; }
    public string? DeveloperNotes { get; set; }
    public string? Notes { get; set; }

    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
