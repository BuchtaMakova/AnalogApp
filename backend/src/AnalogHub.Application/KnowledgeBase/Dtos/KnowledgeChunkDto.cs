using AnalogHub.Domain.Enums;

namespace AnalogHub.Application.KnowledgeBase.Dtos;

public sealed record KnowledgeChunkDto(
    Guid Id,
    string DocumentTitle,
    KnowledgeSourceType SourceType,
    string? SourceUrl,
    int ChunkIndex,
    string Content,
    double SimilarityScore);

public sealed record IngestKnowledgeDocumentResult(string DocumentTitle, int ChunksCreated);

public sealed record KnowledgeCitation(
    Guid ChunkId,
    int CitationNumber,
    string DocumentTitle,
    string? SourceUrl,
    string Excerpt);

public sealed record AskKnowledgeBaseResult(string Answer, IReadOnlyList<KnowledgeCitation> Citations);
