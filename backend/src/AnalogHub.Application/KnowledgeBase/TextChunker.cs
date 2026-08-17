namespace AnalogHub.Application.KnowledgeBase;

/// <summary>
/// Paragraph-aware sliding-window chunker for RAG ingestion. Groups paragraphs up to a character
/// budget (a real tokenizer would be more precise, but ~4 chars/token is a standard approximation
/// for English text and keeps this dependency-free) and carries a small tail of the previous chunk
/// forward so retrieval doesn't lose context at chunk boundaries.
/// </summary>
public static class TextChunker
{
    private const int DefaultMaxChunkChars = 1200;
    private const int DefaultOverlapChars = 200;

    public static IReadOnlyList<string> Chunk(
        string text,
        int maxChunkChars = DefaultMaxChunkChars,
        int overlapChars = DefaultOverlapChars)
    {
        var paragraphs = text
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => p.Length > 0)
            .ToList();

        if (paragraphs.Count == 0)
        {
            return [];
        }

        var chunks = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (current.Length > 0 && current.Length + paragraph.Length + 2 > maxChunkChars)
            {
                var chunkText = current.ToString();
                chunks.Add(chunkText);

                current.Clear();
                current.Append(Tail(chunkText, overlapChars));
                if (current.Length > 0)
                {
                    current.Append("\n\n");
                }
            }

            current.Append(paragraph);
            current.Append("\n\n");
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString().Trim());
        }

        return chunks;
    }

    /// <summary>Approximate token count; a real tokenizer would replace this if precision mattered.</summary>
    public static int EstimateTokenCount(string text) => Math.Max(1, text.Length / 4);

    private static string Tail(string text, int maxChars)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= maxChars ? trimmed : trimmed[^maxChars..];
    }
}
