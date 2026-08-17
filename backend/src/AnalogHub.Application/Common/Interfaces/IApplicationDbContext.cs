using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Common.Interfaces;

/// <summary>
/// Persistence seam the Application layer codes against, so handlers never take a hard dependency
/// on EF Core's <c>DbContext</c> or on the Infrastructure project. Implemented by
/// <c>AnalogHubDbContext</c>.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<CameraBody> CameraBodies { get; }
    DbSet<Lens> Lenses { get; }
    DbSet<Flash> Flashes { get; }
    DbSet<FilmRoll> FilmRolls { get; }
    DbSet<Photo> Photos { get; }
    DbSet<PhotoAiCritique> PhotoAiCritiques { get; }
    DbSet<Tag> Tags { get; }
    DbSet<PhotoTag> PhotoTags { get; }
    DbSet<Album> Albums { get; }
    DbSet<AlbumPhoto> AlbumPhotos { get; }
    DbSet<KnowledgeChunk> KnowledgeChunks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
