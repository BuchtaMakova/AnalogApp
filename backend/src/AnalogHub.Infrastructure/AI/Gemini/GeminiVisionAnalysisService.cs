using System.Text.Json;
using System.Text.Json.Serialization;
using AnalogHub.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AnalogHub.Infrastructure.AI.Gemini;

public sealed class GeminiVisionAnalysisService : IVisionAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonElement CritiqueSchema = JsonSerializer.Deserialize<JsonElement>(PhotoCritiqueGeminiSchema.Schema);

    private readonly GeminiClient _client;
    private readonly HttpClient _downloadClient;
    private readonly string _model;

    public GeminiVisionAnalysisService(GeminiClient client, IHttpClientFactory httpClientFactory, IOptions<GeminiOptions> options)
    {
        _client = client;
        _downloadClient = httpClientFactory.CreateClient();
        _model = options.Value.VisionModel;
    }

    public async Task<PhotoCritiqueResult> AnalyzePhotoAsync(
        string imageUrl,
        VisionAnalysisContext context,
        CancellationToken cancellationToken)
    {
        // Gemini's generateContent only accepts inline base64 bytes (or a prior Files API upload),
        // not an arbitrary external URL — so the presigned download URL has to be fetched here first.
        using var imageResponse = await _downloadClient.GetAsync(imageUrl, cancellationToken);
        imageResponse.EnsureSuccessStatusCode();
        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        var mimeType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        var request = new GeminiGenerateContentRequest(
            Contents:
            [
                new GeminiContent("user",
                [
                    new GeminiPart(Text: BuildUserPrompt(context)),
                    new GeminiPart(InlineData: new GeminiInlineData(mimeType, Convert.ToBase64String(imageBytes)))
                ])
            ],
            SystemInstruction: new GeminiContent(null, [new GeminiPart(Text: BuildSystemPrompt())]),
            // Low, not zero: a critique should read the same photo consistently across re-runs
            // (same score, same named details) without becoming a canned, word-for-word repeat.
            GenerationConfig: new GeminiGenerationConfig(
                ResponseMimeType: "application/json", ResponseSchema: CritiqueSchema, Temperature: 0.2));

        var rawJson = await _client.GenerateContentAsync(_model, request, cancellationToken);

        var payload = JsonSerializer.Deserialize<CritiquePayload>(rawJson, JsonOptions)
            ?? throw new InvalidOperationException("Vision model returned an empty critique payload.");

        return new PhotoCritiqueResult(
            _model,
            payload.CompositionScore,
            payload.CompositionNotes,
            payload.LightingNotes,
            payload.PosingNotes,
            payload.Recommendations,
            payload.SuggestedTags,
            rawJson);
    }

    private static string BuildSystemPrompt() => """
        You are a meticulous analog photography mentor reviewing a scanned film frame. Judge the
        photo on its own merits as a photograph, not the film scan quality. Be specific and
        constructive; reference what is actually visible in the frame. If the subject is not a
        portrait, set posingNotes to null instead of inventing posing feedback.
        """;

    private static string BuildUserPrompt(VisionAnalysisContext context)
    {
        var details = new List<string>();
        if (context.CameraModel is not null) details.Add($"Camera: {context.CameraModel}");
        if (context.LensModel is not null) details.Add($"Lens: {context.LensModel}");
        if (context.FilmStock is not null) details.Add($"Film: {context.FilmStock}");
        if (context.IsoUsed is not null) details.Add($"ISO: {context.IsoUsed}");
        if (context.FlashFired is not null) details.Add($"Flash fired: {context.FlashFired}");

        var detailsBlock = details.Count > 0 ? string.Join(", ", details) : "No gear metadata available.";

        return $"Shooting context — {detailsBlock}. Critique this frame's composition, light/flash " +
               "handling, and posing (if a portrait), then give concrete tips and suggest scene tags.";
    }

    private sealed record CritiquePayload(
        int CompositionScore,
        string CompositionNotes,
        string LightingNotes,
        string? PosingNotes,
        [property: JsonPropertyName("recommendations")] List<string> Recommendations,
        [property: JsonPropertyName("suggestedTags")] List<string> SuggestedTags);
}
