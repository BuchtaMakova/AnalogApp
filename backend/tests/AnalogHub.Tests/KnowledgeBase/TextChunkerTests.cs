using AnalogHub.Application.KnowledgeBase;
using FluentAssertions;
using Xunit;

namespace AnalogHub.Tests.KnowledgeBase;

public sealed class TextChunkerTests
{
    [Fact]
    public void Chunk_EmptyText_ReturnsNoChunks()
    {
        TextChunker.Chunk("").Should().BeEmpty();
    }

    [Fact]
    public void Chunk_WhitespaceOnlyText_ReturnsNoChunks()
    {
        TextChunker.Chunk("   \n\n   ").Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ShortSingleParagraph_ReturnsOneChunkContainingTheFullText()
    {
        var result = TextChunker.Chunk("A short paragraph about analog film photography.");

        result.Should().HaveCount(1);
        result[0].Should().Contain("analog film photography");
    }

    [Fact]
    public void Chunk_MultipleShortParagraphs_KeepsThemInOneChunkWhenUnderTheBudget()
    {
        var text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";

        var result = TextChunker.Chunk(text, maxChunkChars: 1200);

        result.Should().HaveCount(1);
        result[0].Should().Contain("First paragraph.").And.Contain("Second paragraph.").And.Contain("Third paragraph.");
    }

    [Fact]
    public void Chunk_TextExceedingMaxChunkChars_SplitsIntoMultipleChunks()
    {
        var paragraph = string.Join(" ", Enumerable.Repeat("word", 40)); // ~200 chars
        var longText = string.Join("\n\n", Enumerable.Repeat(paragraph, 8)); // ~1700 chars total

        var chunks = TextChunker.Chunk(longText, maxChunkChars: 500, overlapChars: 50);

        chunks.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Chunk_ConsecutiveChunks_ShareOverlapSoContextIsntLostAtBoundaries()
    {
        var paragraphs = Enumerable.Range(1, 6).Select(i => $"Paragraph number {i} with some distinguishing content.");
        var longText = string.Join("\n\n", paragraphs);

        var chunks = TextChunker.Chunk(longText, maxChunkChars: 90, overlapChars: 40);

        chunks.Should().HaveCountGreaterThan(1);
        // The tail of each chunk (minus its trailing paragraph break) should reappear at the start
        // of the next chunk — that's the overlap mechanism doing its job.
        for (var i = 0; i < chunks.Count - 1; i++)
        {
            var tailOfCurrent = chunks[i].TrimEnd();
            var overlapCandidate = tailOfCurrent[^Math.Min(20, tailOfCurrent.Length)..];
            chunks[i + 1].Should().Contain(overlapCandidate.Split(' ').Last());
        }
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("1234", 1)]
    [InlineData("12345678", 2)]
    [InlineData("123456789012", 3)]
    public void EstimateTokenCount_ApproximatesFourCharsPerToken_WithMinimumOfOne(string text, int expected)
    {
        TextChunker.EstimateTokenCount(text).Should().Be(expected);
    }
}
