using AnalogHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photos", t =>
        {
            t.HasCheckConstraint("CK_Photos_Rating_Range", "\"Rating\" >= 0 AND \"Rating\" <= 5");
            t.HasCheckConstraint("CK_Photos_RotationDegrees_Valid", "\"RotationDegrees\" IN (0, 90, 180, 270)");
        });

        builder.Property(p => p.OriginalStorageKey).HasMaxLength(500).IsRequired();
        builder.Property(p => p.PreviewStorageKey).HasMaxLength(500);
        builder.Property(p => p.ThumbnailStorageKey).HasMaxLength(500);
        builder.Property(p => p.BlurHash).HasMaxLength(64);
        builder.Property(p => p.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(p => p.ProcessingStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.ProcessingError).HasMaxLength(2000);

        // 768 dims matches Gemini text-embedding-004.
        builder.Property(p => p.Embedding).HasColumnType("vector(768)");
        builder.HasIndex(p => p.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");

        builder.OwnsOne(p => p.Exif, exif =>
        {
            exif.Property(e => e.Aperture).HasColumnName("Exif_Aperture").HasMaxLength(20);
            exif.Property(e => e.ShutterSpeed).HasColumnName("Exif_ShutterSpeed").HasMaxLength(20);
            exif.Property(e => e.IsoUsed).HasColumnName("Exif_IsoUsed");
            exif.Property(e => e.FocalLengthMm).HasColumnName("Exif_FocalLengthMm").HasPrecision(6, 1);
            exif.Property(e => e.FlashFired).HasColumnName("Exif_FlashFired");
            exif.Property(e => e.MeteringMode).HasColumnName("Exif_MeteringMode").HasMaxLength(50);
            exif.Property(e => e.ScannerModel).HasColumnName("Exif_ScannerModel").HasMaxLength(100);
            exif.Property(e => e.GpsLatitude).HasColumnName("Exif_GpsLatitude");
            exif.Property(e => e.GpsLongitude).HasColumnName("Exif_GpsLongitude");
        });

        builder.HasOne(p => p.Critique)
            .WithOne(c => c.Photo)
            .HasForeignKey<PhotoAiCritique>(c => c.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.FilmRollId);
        builder.HasIndex(p => p.ProcessingStatus);
        builder.HasIndex(p => p.CaptureDateUtc);

        builder.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.UserId);
    }
}
