using AnalogHub.Domain.Common;

namespace AnalogHub.Domain.Entities.Gear;

/// <summary>
/// Base of a TPH (table-per-hierarchy) gear catalogue. <see cref="CameraBody"/>, <see cref="Lens"/>
/// and <see cref="Flash"/> all live in a single "Gear" table discriminated by GearType, which lets
/// Photo/FilmRoll hold plain FK columns into one table instead of three.
/// </summary>
public abstract class Gear : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Brand { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string? SerialNumber { get; set; }
    public string? MountType { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? AcquiredOn { get; set; }
}
