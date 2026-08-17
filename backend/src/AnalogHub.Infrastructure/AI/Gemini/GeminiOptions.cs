namespace AnalogHub.Infrastructure.AI.Gemini;

/// <summary>
/// Bound from configuration section "Gemini". One API key drives vision critique, RAG chat and
/// embeddings. Google rotates model availability aggressively — a "no longer available" 404 means
/// the default here has aged out; check https://ai.google.dev/gemini-api/docs/models or call
/// GET https://generativelanguage.googleapis.com/v1beta/models?key=... for the current lineup.
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = default!;

    /// <summary>Multimodal model used for photo critique (needs vision support).</summary>
    public string VisionModel { get; set; } = "gemini-3.6-flash";

    /// <summary>Text model used for the RAG assistant chat.</summary>
    public string ChatModel { get; set; } = "gemini-3.6-flash";

    /// <summary>
    /// Produces up to 3072-dim vectors natively; <see cref="AI.Gemini.GeminiClient"/> requests
    /// outputDimensionality=768 to match the pgvector column width (gemini-embedding-001 supports
    /// Matryoshka truncation, so this stays a good-quality embedding, not a naive slice).
    /// </summary>
    public string EmbeddingModel { get; set; } = "gemini-embedding-001";
}
