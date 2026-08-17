using AnalogHub.Domain.Common;

namespace AnalogHub.Domain.Entities;

public sealed class Album : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public Guid? CoverPhotoId { get; set; }
    public Photo? CoverPhoto { get; set; }

    public ICollection<AlbumPhoto> AlbumPhotos { get; set; } = new List<AlbumPhoto>();
}
