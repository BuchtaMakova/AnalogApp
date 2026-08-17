using AnalogHub.Domain.Common;

namespace AnalogHub.Domain.Entities;

public sealed class Tag : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;

    public ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
}
