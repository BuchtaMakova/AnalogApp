using AnalogHub.Application.Common.Interfaces;

namespace AnalogHub.Infrastructure.AI.Mock;

/// <summary>
/// Demo-mode stand-in for <see cref="IEmbeddingService"/>. Not a real embedding model — uses the
/// hashing trick (each lowercased word hashes into one of 768 dimensions with a signed weight, then
/// the vector is L2-normalized) to produce a deterministic, keyword-sensitive vector. Cosine
/// similarity between two texts roughly tracks vocabulary overlap, which is enough for the RAG
/// pipeline's retrieval mechanics to demonstrably work end-to-end without a Gemini API key — it is
/// not a substitute for real semantic similarity.
/// </summary>
public sealed class MockEmbeddingService : IEmbeddingService
{
    private const int Dimensions = 768;

    private static readonly char[] SplitChars = [' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '"', '\''];

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken) =>
        Task.FromResult(ComputeEmbedding(text));

    public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<float[]>>(texts.Select(ComputeEmbedding).ToList());

    private static float[] ComputeEmbedding(string text)
    {
        var vector = new float[Dimensions];
        var words = text.ToLowerInvariant().Split(
            SplitChars,
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var hash = word.GetHashCode(StringComparison.Ordinal);
            var index = Math.Abs(hash) % Dimensions;
            var sign = (hash & 1) == 0 ? 1f : -1f;
            vector[index] += sign;
        }

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        if (magnitude > 0f)
        {
            for (var i = 0; i < vector.Length; i++)
            {
                vector[i] /= magnitude;
            }
        }

        return vector;
    }
}
