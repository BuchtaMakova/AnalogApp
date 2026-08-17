using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.KnowledgeBase.Dtos;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace AnalogHub.Application.KnowledgeBase.Commands;

/// <summary>
/// Ingests a knowledge-base article: chunks the raw text, embeds every chunk, and stores them as
/// <see cref="KnowledgeChunk"/> rows for RAG retrieval. Re-ingesting the same DocumentTitle replaces
/// its previous chunks so edits don't leave stale vectors behind.
/// </summary>
public sealed record IngestKnowledgeDocumentCommand(
    string DocumentTitle,
    KnowledgeSourceType SourceType,
    string? SourceUrl,
    string Content) : IRequest<IngestKnowledgeDocumentResult>;

public sealed class IngestKnowledgeDocumentCommandValidator : AbstractValidator<IngestKnowledgeDocumentCommand>
{
    public IngestKnowledgeDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentTitle).NotEmpty().MaximumLength(300);
        RuleFor(x => x.SourceType).IsInEnum();
        RuleFor(x => x.SourceUrl).MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(50);
    }
}

public sealed class IngestKnowledgeDocumentCommandHandler
    : IRequestHandler<IngestKnowledgeDocumentCommand, IngestKnowledgeDocumentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmbeddingService _embeddings;

    public IngestKnowledgeDocumentCommandHandler(IApplicationDbContext db, IEmbeddingService embeddings)
    {
        _db = db;
        _embeddings = embeddings;
    }

    public async Task<IngestKnowledgeDocumentResult> Handle(
        IngestKnowledgeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var existingChunks = await _db.KnowledgeChunks
            .Where(k => k.DocumentTitle == request.DocumentTitle)
            .ToListAsync(cancellationToken);
        if (existingChunks.Count > 0)
        {
            _db.KnowledgeChunks.RemoveRange(existingChunks);
        }

        var chunkTexts = TextChunker.Chunk(request.Content);
        var embeddings = await _embeddings.EmbedBatchAsync(chunkTexts, cancellationToken);

        for (var i = 0; i < chunkTexts.Count; i++)
        {
            _db.KnowledgeChunks.Add(new KnowledgeChunk
            {
                DocumentTitle = request.DocumentTitle,
                SourceType = request.SourceType,
                SourceUrl = request.SourceUrl,
                ChunkIndex = i,
                Content = chunkTexts[i],
                TokenCount = TextChunker.EstimateTokenCount(chunkTexts[i]),
                Embedding = new Vector(embeddings[i])
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new IngestKnowledgeDocumentResult(request.DocumentTitle, chunkTexts.Count);
    }
}
