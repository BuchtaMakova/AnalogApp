namespace AnalogHub.Application.Common.Interfaces;

/// <summary>Text embedding generation for RAG ingestion and query-time retrieval. Must produce 1536-dim vectors.</summary>
public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken);

    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken);
}
