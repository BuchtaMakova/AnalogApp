using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.KnowledgeBase.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace AnalogHub.Application.KnowledgeBase.Queries;

/// <summary>Cosine-similarity search over the pgvector-embedded knowledge base.</summary>
public sealed record SearchKnowledgeChunksQuery(string Query, int TopK = 5) : IRequest<IReadOnlyList<KnowledgeChunkDto>>;

public sealed class SearchKnowledgeChunksQueryHandler
    : IRequestHandler<SearchKnowledgeChunksQuery, IReadOnlyList<KnowledgeChunkDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmbeddingService _embeddings;

    public SearchKnowledgeChunksQueryHandler(IApplicationDbContext db, IEmbeddingService embeddings)
    {
        _db = db;
        _embeddings = embeddings;
    }

    public async Task<IReadOnlyList<KnowledgeChunkDto>> Handle(
        SearchKnowledgeChunksQuery request,
        CancellationToken cancellationToken)
    {
        var queryEmbedding = new Vector(await _embeddings.EmbedAsync(request.Query, cancellationToken));

        var results = await _db.KnowledgeChunks
            .AsNoTracking()
            .OrderBy(k => k.Embedding.CosineDistance(queryEmbedding))
            .Take(request.TopK)
            .Select(k => new
            {
                k.Id,
                k.DocumentTitle,
                k.SourceType,
                k.SourceUrl,
                k.ChunkIndex,
                k.Content,
                Distance = k.Embedding.CosineDistance(queryEmbedding)
            })
            .ToListAsync(cancellationToken);

        return results
            .Select(r => new KnowledgeChunkDto(
                r.Id, r.DocumentTitle, r.SourceType, r.SourceUrl, r.ChunkIndex, r.Content, 1 - r.Distance))
            .ToList();
    }
}
