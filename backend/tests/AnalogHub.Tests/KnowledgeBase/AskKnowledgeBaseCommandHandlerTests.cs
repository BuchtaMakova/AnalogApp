using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.KnowledgeBase.Commands;
using AnalogHub.Application.KnowledgeBase.Dtos;
using AnalogHub.Application.KnowledgeBase.Queries;
using AnalogHub.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace AnalogHub.Tests.KnowledgeBase;

/// <summary>
/// The real cosine-similarity retrieval (<c>SearchKnowledgeChunksQueryHandler</c>) runs a pgvector
/// query that only Postgres can translate, so it's exercised via manual/integration testing against
/// a real database, not here. These tests cover the handler's own logic — grounding the prompt in
/// retrieved chunks, building numbered citations, and the empty-knowledge-base fallback — by mocking
/// the retrieval step (<see cref="ISender"/>) rather than hitting a database at all.
/// </summary>
public sealed class AskKnowledgeBaseCommandHandlerTests
{
    private static KnowledgeChunkDto CreateChunk(string title, string content, double score = 0.85) =>
        new(Guid.NewGuid(), title, KnowledgeSourceType.Technique, null, 0, content, score);

    [Fact]
    public async Task Handle_NoChunksRetrieved_ReturnsFriendlyMessageWithoutCallingChatCompletion()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<SearchKnowledgeChunksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KnowledgeChunkDto>());

        var chatCompletion = new Mock<IChatCompletionService>();
        var handler = new AskKnowledgeBaseCommandHandler(sender.Object, chatCompletion.Object);

        var result = await handler.Handle(new AskKnowledgeBaseCommand("What film should I use for portraits?"), CancellationToken.None);

        result.Citations.Should().BeEmpty();
        result.Answer.Should().Contain("ingest");
        chatCompletion.Verify(
            c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ChunksRetrieved_BuildsNumberedCitationsMatchingRetrievalOrder()
    {
        var chunks = new List<KnowledgeChunkDto>
        {
            CreateChunk("Manual Flash Technique for Film Photography", "Guide numbers explain flash exposure math."),
            CreateChunk("Portrait Composition Fundamentals", "Rule of thirds places eyes on power points."),
        };

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<SearchKnowledgeChunksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);

        var chatCompletion = new Mock<IChatCompletionService>();
        chatCompletion
            .Setup(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Use the guide number formula [1] to compute aperture.");

        var handler = new AskKnowledgeBaseCommandHandler(sender.Object, chatCompletion.Object);

        var result = await handler.Handle(new AskKnowledgeBaseCommand("How do I calculate flash exposure?"), CancellationToken.None);

        result.Answer.Should().Contain("[1]");
        result.Citations.Should().HaveCount(2);
        result.Citations[0].CitationNumber.Should().Be(1);
        result.Citations[0].DocumentTitle.Should().Be("Manual Flash Technique for Film Photography");
        result.Citations[1].CitationNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ChunksRetrieved_GroundsTheChatPromptInTheRetrievedContext()
    {
        var chunks = new List<KnowledgeChunkDto> { CreateChunk("35mm vs 120", "Medium format has a shallower depth of field.") };

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<SearchKnowledgeChunksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);

        var chatCompletion = new Mock<IChatCompletionService>();
        chatCompletion
            .Setup(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("answer");

        var handler = new AskKnowledgeBaseCommandHandler(sender.Object, chatCompletion.Object);

        await handler.Handle(new AskKnowledgeBaseCommand("Which format has shallower depth of field?"), CancellationToken.None);

        chatCompletion.Verify(c => c.CompleteAsync(
            It.Is<string>(systemPrompt => systemPrompt.Contains("35mm vs 120") && systemPrompt.Contains("shallower depth of field")),
            "Which format has shallower depth of field?",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PassesRequestedTopKThroughToRetrieval()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<SearchKnowledgeChunksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KnowledgeChunkDto>());

        var handler = new AskKnowledgeBaseCommandHandler(sender.Object, Mock.Of<IChatCompletionService>());

        await handler.Handle(new AskKnowledgeBaseCommand("question", TopK: 3), CancellationToken.None);

        sender.Verify(s => s.Send(
            It.Is<SearchKnowledgeChunksQuery>(q => q.TopK == 3 && q.Query == "question"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
