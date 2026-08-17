using AnalogHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(120).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}

public sealed class PhotoTagConfiguration : IEntityTypeConfiguration<PhotoTag>
{
    public void Configure(EntityTypeBuilder<PhotoTag> builder)
    {
        builder.ToTable("PhotoTags");

        builder.HasKey(pt => new { pt.PhotoId, pt.TagId });

        builder.HasOne(pt => pt.Photo)
            .WithMany(p => p.PhotoTags)
            .HasForeignKey(pt => pt.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.PhotoTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
