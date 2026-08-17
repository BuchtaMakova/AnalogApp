using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Tests.Common;

/// <summary>
/// A minimal, provider-agnostic <see cref="IApplicationDbContext"/> for handler unit tests —
/// deliberately separate from Infrastructure's <c>AnalogHubDbContext</c>, which configures
/// pgvector-specific column types and HNSW indexes that only Npgsql understands. EF Core's InMemory
/// provider can't translate <c>CosineDistance</c> or store <see cref="Pgvector.Vector"/> columns, so
/// vector properties are excluded here — handlers that do real vector search (e.g.
/// <c>SearchKnowledgeChunksQueryHandler</c>) are exercised via mocked <c>ISender</c> results in
/// tests instead of a real database, not through this context.
/// </summary>
public sealed class TestDbContext : DbContext, IApplicationDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<CameraBody> CameraBodies => Set<CameraBody>();
    public DbSet<Lens> Lenses => Set<Lens>();
    public DbSet<Flash> Flashes => Set<Flash>();
    public DbSet<FilmRoll> FilmRolls => Set<FilmRoll>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<PhotoAiCritique> PhotoAiCritiques => Set<PhotoAiCritique>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PhotoTag> PhotoTags => Set<PhotoTag>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<AlbumPhoto> AlbumPhotos => Set<AlbumPhoto>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Gear>().HasDiscriminator<string>("GearType")
            .HasValue<CameraBody>("CameraBody")
            .HasValue<Lens>("Lens")
            .HasValue<Flash>("Flash");

        modelBuilder.Entity<Photo>(photo =>
        {
            photo.OwnsOne(p => p.Exif);
            photo.Ignore(p => p.Embedding);
        });

        modelBuilder.Entity<KnowledgeChunk>().Ignore(k => k.Embedding);

        modelBuilder.Entity<PhotoTag>().HasKey(pt => new { pt.PhotoId, pt.TagId });
        modelBuilder.Entity<AlbumPhoto>().HasKey(ap => new { ap.AlbumId, ap.PhotoId });

        base.OnModelCreating(modelBuilder);
    }
}

public static class TestDbContextFactory
{
    /// <summary>Fresh, isolated in-memory database per call — safe to use one per test.</summary>
    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }
}
