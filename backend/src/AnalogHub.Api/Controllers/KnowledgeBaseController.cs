using AnalogHub.Application.KnowledgeBase.Commands;
using AnalogHub.Application.KnowledgeBase.Dtos;
using AnalogHub.Application.KnowledgeBase.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/knowledge-base")]
public sealed class KnowledgeBaseController : ControllerBase
{
    private readonly ISender _sender;

    public KnowledgeBaseController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Ingests (or re-ingests) an article: chunks the text and embeds each chunk for retrieval.</summary>
    [HttpPost("documents")]
    [ProducesResponseType(typeof(IngestKnowledgeDocumentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<IngestKnowledgeDocumentResult>> IngestDocument(
        [FromBody] IngestKnowledgeDocumentCommand command, CancellationToken cancellationToken)
        => Ok(await _sender.Send(command, cancellationToken));

    /// <summary>Raw cosine-similarity search over the knowledge base, without an LLM answer.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<KnowledgeChunkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<KnowledgeChunkDto>>> Search(
        [FromQuery] string query, [FromQuery] int topK, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new SearchKnowledgeChunksQuery(query, topK == 0 ? 5 : topK), cancellationToken));

    /// <summary>RAG chat: answers a technique/gear question grounded in the knowledge base with citations.</summary>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AskKnowledgeBaseResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AskKnowledgeBaseResult>> Ask(
        [FromBody] AskKnowledgeBaseCommand command, CancellationToken cancellationToken)
        => Ok(await _sender.Send(command, cancellationToken));
}
