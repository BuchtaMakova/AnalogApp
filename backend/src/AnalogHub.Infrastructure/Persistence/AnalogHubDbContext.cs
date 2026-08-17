using System.Reflection;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Infrastructure.Persistence;

public sealed class AnalogHubDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditInterceptor;

    public AnalogHubDbContext(
        DbContextOptions<AnalogHubDbContext> options,
        AuditableEntitySaveChangesInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}
