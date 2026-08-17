namespace AnalogHub.Domain.Entities;

public sealed class AlbumPhoto
{
    public Guid AlbumId { get; set; }
    public Album Album { get; set; } = default!;

    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = default!;

    public int SortOrder { get; set; }
}
