namespace AnalogHub.Domain.Entities.Gear;

using AnalogHub.Domain.Entities;

public sealed class Lens : Gear
{
    public decimal? FocalLengthMinMm { get; set; }
    public decimal? FocalLengthMaxMm { get; set; }
    public decimal? MaxAperture { get; set; }
    public decimal? MinAperture { get; set; }

    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
