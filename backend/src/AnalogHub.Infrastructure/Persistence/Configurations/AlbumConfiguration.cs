using AnalogHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("Albums");

        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);

        builder.HasOne(a => a.CoverPhoto)
            .WithMany()
            .HasForeignKey(a => a.CoverPhotoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class AlbumPhotoConfiguration : IEntityTypeConfiguration<AlbumPhoto>
{
    public void Configure(EntityTypeBuilder<AlbumPhoto> builder)
    {
        builder.ToTable("AlbumPhotos");

        builder.HasKey(ap => new { ap.AlbumId, ap.PhotoId });

        builder.HasOne(ap => ap.Album)
            .WithMany(a => a.AlbumPhotos)
            .HasForeignKey(ap => ap.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.Photo)
            .WithMany(p => p.AlbumPhotos)
            .HasForeignKey(ap => ap.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ap => new { ap.AlbumId, ap.SortOrder });
    }
}
