using AnalogHub.Domain.Common;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using AnalogHub.Domain.ValueObjects;
using Pgvector;

namespace AnalogHub.Domain.Entities;

/// <summary>
/// A single scanned frame. Storage keys point at objects in S3/R2 (original, preview, thumbnail);
/// <see cref="Embedding"/> is a pgvector column used for visual similarity search once AI critique
/// has run. Rows are created in <see cref="PhotoProcessingStatus.PendingUpload"/> or
/// <see cref="PhotoProcessingStatus.Uploaded"/> and move to Ready once the background job finishes
/// generating derivatives and BlurHash.
/// </summary>
public sealed class Photo : AuditableEntity
{
    public Guid FilmRollId { get; set; }
    public FilmRoll FilmRoll { get; set; } = default!;

    public Guid? CameraBodyId { get; set; }
    public CameraBody? CameraBody { get; set; }

    public Guid? LensId { get; set; }
    public Lens? Lens { get; set; }

    public Guid? FlashId { get; set; }
    public Flash? Flash { get; set; }

    public int? FrameNumber { get; set; }
    public DateTimeOffset? CaptureDateUtc { get; set; }

    public string OriginalStorageKey { get; set; } = default!;
    public string? PreviewStorageKey { get; set; }
    public string? ThumbnailStorageKey { get; set; }

    /// <summary>Compact placeholder string decoded client-side while the WebP preview loads.</summary>
    public string? BlurHash { get; set; }

    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = "image/jpeg";

    /// <summary>User rating, 0 (unrated) to 5.</summary>
    public byte Rating { get; set; }

    public PhotoProcessingStatus ProcessingStatus { get; set; } = PhotoProcessingStatus.PendingUpload;
    public string? ProcessingError { get; set; }

    public ExifData Exif { get; set; } = new();

    /// <summary>Visual embedding for similarity search, populated after AI critique. 768 dims (Gemini text-embedding-004 size).</summary>
    public Vector? Embedding { get; set; }

    public PhotoAiCritique? Critique { get; set; }

    public ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
    public ICollection<AlbumPhoto> AlbumPhotos { get; set; } = new List<AlbumPhoto>();
}
