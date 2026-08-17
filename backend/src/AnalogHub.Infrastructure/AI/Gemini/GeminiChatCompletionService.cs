using AnalogHub.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AnalogHub.Infrastructure.AI.Gemini;

public sealed class GeminiChatCompletionService : IChatCompletionService
{
    private readonly GeminiClient _client;
    private readonly string _model;

    public GeminiChatCompletionService(GeminiClient client, IOptions<GeminiOptions> options)
    {
        _client = client;
        _model = options.Value.ChatModel;
    }

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var request = new GeminiGenerateContentRequest(
            Contents: [new GeminiContent("user", [new GeminiPart(Text: userPrompt)])],
            SystemInstruction: new GeminiContent(null, [new GeminiPart(Text: systemPrompt)]));

        return _client.GenerateContentAsync(_model, request, cancellationToken);
    }
}
