using System.Text;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.KnowledgeBase.Dtos;
using AnalogHub.Application.KnowledgeBase.Queries;
using FluentValidation;
using MediatR;

namespace AnalogHub.Application.KnowledgeBase.Commands;

/// <summary>
/// RAG chat: retrieves the most relevant knowledge chunks for the question, grounds the LLM's
/// answer in them via numbered citations, and returns both the answer and the cited sources so the
/// UI can render "[1]"-style references back to their documents.
/// </summary>
public sealed record AskKnowledgeBaseCommand(string Question, int TopK = 5) : IRequest<AskKnowledgeBaseResult>;

public sealed class AskKnowledgeBaseCommandValidator : AbstractValidator<AskKnowledgeBaseCommand>
{
    public AskKnowledgeBaseCommandValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TopK).InclusiveBetween(1, 20);
    }
}

public sealed class AskKnowledgeBaseCommandHandler : IRequestHandler<AskKnowledgeBaseCommand, AskKnowledgeBaseResult>
{
    private readonly ISender _sender;
    private readonly IChatCompletionService _chatCompletion;

    public AskKnowledgeBaseCommandHandler(ISender sender, IChatCompletionService chatCompletion)
    {
        _sender = sender;
        _chatCompletion = chatCompletion;
    }

    public async Task<AskKnowledgeBaseResult> Handle(AskKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var chunks = await _sender.Send(new SearchKnowledgeChunksQuery(request.Question, request.TopK), cancellationToken);

        if (chunks.Count == 0)
        {
            return new AskKnowledgeBaseResult(
                "I don't have any knowledge base articles to answer that yet — ingest some technique guides first.",
                []);
        }

        var systemPrompt = BuildSystemPrompt(chunks);
        var answer = await _chatCompletion.CompleteAsync(systemPrompt, request.Question, cancellationToken);

        var citations = chunks
            .Select((c, i) => new KnowledgeCitation(c.Id, i + 1, c.DocumentTitle, c.SourceUrl, Excerpt(c.Content)))
            .ToList();

        return new AskKnowledgeBaseResult(answer, citations);
    }

    private static string BuildSystemPrompt(IReadOnlyList<KnowledgeChunkDto> chunks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            You are the Analog Darkroom Assistant, an expert on analog photography technique, gear
            and darkroom process. Answer the user's question using ONLY the numbered context below.
            Cite every claim with its bracketed number, e.g. [1]. If the context does not contain the
            answer, say so plainly instead of guessing.
            """);
        sb.AppendLine();
        sb.AppendLine("Context:");

        for (var i = 0; i < chunks.Count; i++)
        {
            sb.AppendLine($"[{i + 1}] ({chunks[i].DocumentTitle}) {chunks[i].Content}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Excerpt(string content, int maxChars = 220) =>
        content.Length <= maxChars ? content : content[..maxChars].TrimEnd() + "…";
}
