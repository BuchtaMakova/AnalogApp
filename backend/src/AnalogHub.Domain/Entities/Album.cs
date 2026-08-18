using AnalogHub.Domain.Common;

namespace AnalogHub.Domain.Entities;

public sealed class Album : AuditableEntity
{
    /// <summary>Owner of this album — every user has their own, never a shared one.</summary>
    public Guid UserId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public Guid? CoverPhotoId { get; set; }
    public Photo? CoverPhoto { get; set; }

    public ICollection<AlbumPhoto> AlbumPhotos { get; set; } = new List<AlbumPhoto>();
}
