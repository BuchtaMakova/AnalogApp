namespace AnalogHub.Application.Common.Interfaces;

/// <summary>Plain text chat completion, used by the RAG assistant to answer over retrieved knowledge chunks.</summary>
public interface IChatCompletionService
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}
