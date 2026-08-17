using AnalogHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeChunkConfiguration : IEntityTypeConfiguration<KnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<KnowledgeChunk> builder)
    {
        builder.ToTable("KnowledgeChunks");

        builder.Property(k => k.DocumentTitle).HasMaxLength(300).IsRequired();
        builder.Property(k => k.SourceType).HasConversion<string>().HasMaxLength(30);
        builder.Property(k => k.SourceUrl).HasMaxLength(500);
        builder.Property(k => k.Content).IsRequired();

        // 768 dims matches Gemini text-embedding-004.
        builder.Property(k => k.Embedding).HasColumnType("vector(768)").IsRequired();
        builder.HasIndex(k => k.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");

        builder.HasIndex(k => new { k.DocumentTitle, k.ChunkIndex });
    }
}
