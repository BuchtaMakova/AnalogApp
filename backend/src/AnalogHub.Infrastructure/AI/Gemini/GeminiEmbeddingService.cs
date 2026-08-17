using AnalogHub.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AnalogHub.Infrastructure.AI.Gemini;

public sealed class GeminiEmbeddingService : IEmbeddingService
{
    private readonly GeminiClient _client;
    private readonly string _model;

    public GeminiEmbeddingService(GeminiClient client, IOptions<GeminiOptions> options)
    {
        _client = client;
        _model = options.Value.EmbeddingModel;
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
        => _client.EmbedContentAsync(_model, text, cancellationToken);

    public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken)
        => _client.BatchEmbedContentsAsync(_model, texts, cancellationToken);
}
