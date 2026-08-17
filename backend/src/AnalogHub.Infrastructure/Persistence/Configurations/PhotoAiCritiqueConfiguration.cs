using AnalogHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class PhotoAiCritiqueConfiguration : IEntityTypeConfiguration<PhotoAiCritique>
{
    public void Configure(EntityTypeBuilder<PhotoAiCritique> builder)
    {
        builder.ToTable("PhotoAiCritiques");

        builder.Property(c => c.Model).HasMaxLength(100).IsRequired();
        builder.Property(c => c.RecommendationsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(c => c.SuggestedTagsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(c => c.RawResponseJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(c => c.PhotoId).IsUnique();
    }
}
