namespace AnalogHub.Domain.Entities.Gear;

using AnalogHub.Domain.Entities;

public sealed class Flash : Gear
{
    public decimal? GuideNumber { get; set; }
    public bool HasTtl { get; set; }

    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
