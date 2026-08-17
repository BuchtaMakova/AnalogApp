using AnalogHub.Domain.Common;
using AnalogHub.Domain.Enums;
using Pgvector;

namespace AnalogHub.Domain.Entities;

/// <summary>
/// A chunked, embedded slice of analog-photography reference material (manuals, technique guides,
/// posing checklists) used by the RAG assistant. Retrieval is a cosine-similarity search over
/// <see cref="Embedding"/> (768 dims), then the matched chunks are fed to the chat LLM as context.
/// </summary>
public sealed class KnowledgeChunk : AuditableEntity
{
    public string DocumentTitle { get; set; } = default!;
    public KnowledgeSourceType SourceType { get; set; }
    public string? SourceUrl { get; set; }

    /// <summary>Position of this chunk within the source document, for reassembly/citation.</summary>
    public int ChunkIndex { get; set; }

    public string Content { get; set; } = default!;
    public int TokenCount { get; set; }

    public Vector Embedding { get; set; } = default!;
}
