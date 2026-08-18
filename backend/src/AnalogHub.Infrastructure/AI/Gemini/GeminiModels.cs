using System.Text.Json;

namespace AnalogHub.Infrastructure.AI.Gemini;

// Wire types for the Gemini "generateContent" / "embedContent" / "batchEmbedContents" REST
// endpoints (https://ai.google.dev/api). Serialized with System.Text.Json's Web (camelCase) naming
// policy, which matches the API's camelCase field names one-to-one with these PascalCase names.

// These four are constructed directly by the Gemini*Service classes when building a request, so
// (unlike the rest of this file) they need to be as visible as GeminiClient's public API surface.
public sealed record GeminiPart(string? Text = null, GeminiInlineData? InlineData = null);

public sealed record GeminiInlineData(string MimeType, string Data);

public sealed record GeminiContent(string? Role, IReadOnlyList<GeminiPart> Parts);

public sealed record GeminiGenerationConfig(
    string? ResponseMimeType = null,
    JsonElement? ResponseSchema = null,
    double? Temperature = null);

public sealed record GeminiGenerateContentRequest(
    IReadOnlyList<GeminiContent> Contents,
    GeminiContent? SystemInstruction = null,
    GeminiGenerationConfig? GenerationConfig = null);

internal sealed record GeminiGenerateContentResponse(IReadOnlyList<GeminiCandidate>? Candidates);

internal sealed record GeminiCandidate(GeminiContent? Content);

internal sealed record GeminiEmbedContentRequest(GeminiContent Content, int? OutputDimensionality = null);

internal sealed record GeminiEmbedContentResponse(GeminiEmbeddingValues? Embedding);

internal sealed record GeminiEmbeddingValues(IReadOnlyList<float>? Values);

internal sealed record GeminiBatchEmbedRequestItem(string Model, GeminiContent Content, int? OutputDimensionality = null);

internal sealed record GeminiBatchEmbedContentsRequest(IReadOnlyList<GeminiBatchEmbedRequestItem> Requests);

internal sealed record GeminiBatchEmbedResponse(IReadOnlyList<GeminiEmbeddingValues>? Embeddings);
