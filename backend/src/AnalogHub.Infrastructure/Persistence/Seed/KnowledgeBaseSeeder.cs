using AnalogHub.Application.KnowledgeBase.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnalogHub.Infrastructure.Persistence.Seed;

/// <summary>
/// Ingests the seed articles through the real <see cref="IngestKnowledgeDocumentCommand"/> pipeline
/// (chunk + embed), so the RAG assistant can answer real questions out of the box. Requires a valid
/// Gemini API key — failures are logged as warnings rather than crashing startup, since a missing key
/// shouldn't block the rest of the app from running.
/// </summary>
internal static class KnowledgeBaseSeeder
{
    public static async Task SeedAsync(AnalogHubDbContext db, ISender sender, ILogger logger, CancellationToken cancellationToken)
    {
        if (await db.KnowledgeChunks.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Knowledge base seed skipped — KnowledgeChunks already populated.");
            return;
        }

        foreach (var article in KnowledgeBaseSeedArticles.All)
        {
            try
            {
                var result = await sender.Send(
                    new IngestKnowledgeDocumentCommand(article.Title, article.SourceType, article.SourceUrl, article.Content),
                    cancellationToken);

                logger.LogInformation("Ingested knowledge base article '{Title}' ({Chunks} chunks).", result.DocumentTitle, result.ChunksCreated);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Skipped seeding knowledge base article '{Title}' — is Gemini:ApiKey configured?",
                    article.Title);
                return;
            }
        }
    }
}
