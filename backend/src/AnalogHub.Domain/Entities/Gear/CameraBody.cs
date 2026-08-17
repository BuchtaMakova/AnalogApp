using AnalogHub.Domain.Enums;

namespace AnalogHub.Domain.Entities.Gear;

using AnalogHub.Domain.Entities;

public sealed class CameraBody : Gear
{
    /// <summary>Film formats this body can physically load (e.g. a 35mm body only accepts 35mm).</summary>
    public List<FilmFormat> SupportedFormats { get; set; } = new();

    public ICollection<FilmRoll> FilmRolls { get; set; } = new List<FilmRoll>();
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
