using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace AnalogHub.Infrastructure.AI.Gemini;

/// <summary>
/// Thin wrapper over the Gemini REST API (https://ai.google.dev/api) shared by the vision, chat and
/// embedding services — handles auth, JSON (de)serialization and error surfacing in one place.
/// Public because it appears in the constructor signature of the public Gemini*Service classes;
/// still Infrastructure-internal in spirit (not exposed via any Application-layer interface).
/// </summary>
public sealed class GeminiClient
{
    // Must match the pgvector column width (see PhotoConfiguration / KnowledgeChunkConfiguration).
    // gemini-embedding-001's native output is 3072 dims; outputDimensionality asks it to truncate
    // (Matryoshka representation) down to this size instead.
    private const int EmbeddingDimensions = 768;

    private const string ApiKeyHeader = "x-goog-api-key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiClient(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://generativelanguage.googleapis.com/v1beta/");
        _options = options.Value;
    }

    public async Task<string> GenerateContentAsync(
        string model,
        GeminiGenerateContentRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await PostAsync($"models/{model}:generateContent", request, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini generateContent failed ({(int)response.StatusCode}): {body}");
        }

        var parsed = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(body, JsonOptions);
        var text = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        return text ?? throw new InvalidOperationException($"Gemini returned no content: {body}");
    }

    public async Task<float[]> EmbedContentAsync(string model, string text, CancellationToken cancellationToken)
    {
        var request = new GeminiEmbedContentRequest(new GeminiContent(null, [new GeminiPart(Text: text)]), EmbeddingDimensions);

        using var response = await PostAsync($"models/{model}:embedContent", request, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini embedContent failed ({(int)response.StatusCode}): {body}");
        }

        var parsed = JsonSerializer.Deserialize<GeminiEmbedContentResponse>(body, JsonOptions);
        var values = parsed?.Embedding?.Values;

        return values?.ToArray() ?? throw new InvalidOperationException($"Gemini returned no embedding: {body}");
    }

    public async Task<IReadOnlyList<float[]>> BatchEmbedContentsAsync(
        string model,
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
        {
            return [];
        }

        var items = texts
            .Select(text => new GeminiBatchEmbedRequestItem(
                $"models/{model}", new GeminiContent(null, [new GeminiPart(Text: text)]), EmbeddingDimensions))
            .ToList();

        using var response = await PostAsync(
            $"models/{model}:batchEmbedContents", new GeminiBatchEmbedContentsRequest(items), cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini batchEmbedContents failed ({(int)response.StatusCode}): {body}");
        }

        var parsed = JsonSerializer.Deserialize<GeminiBatchEmbedResponse>(body, JsonOptions);

        return parsed?.Embeddings?.Select(e => e.Values?.ToArray() ?? []).ToList()
            ?? throw new InvalidOperationException($"Gemini returned no embeddings: {body}");
    }

    /// <summary>
    /// Sends the key via the x-goog-api-key header rather than a ?key= query string — the query
    /// string form ends up verbatim in ASP.NET Core's HttpClient request-URI logging.
    /// </summary>
    private async Task<HttpResponseMessage> PostAsync<TRequest>(string path, TRequest request, CancellationToken cancellationToken)
    {
        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        httpRequest.Headers.Add(ApiKeyHeader, _options.ApiKey);

        return await _httpClient.SendAsync(httpRequest, cancellationToken);
    }
}
