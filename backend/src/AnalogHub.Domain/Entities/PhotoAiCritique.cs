using AnalogHub.Domain.Common;

namespace AnalogHub.Domain.Entities;

/// <summary>
/// Structured output of a multimodal LLM vision pass over a <see cref="Photo"/>: composition,
/// light/flash handling, posing (for portraits), and recommendations. One-to-one with Photo;
/// re-running critique replaces this row rather than appending history.
/// </summary>
public sealed class PhotoAiCritique : AuditableEntity
{
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = default!;

    /// <summary>Model identifier used for the critique, e.g. "gpt-4o" or "claude-3-5-sonnet".</summary>
    public string Model { get; set; } = default!;

    public int? CompositionScore { get; set; }
    public string? CompositionNotes { get; set; }
    public string? LightingNotes { get; set; }
    public string? PosingNotes { get; set; }

    /// <summary>JSON array of free-text recommendation strings.</summary>
    public string RecommendationsJson { get; set; } = "[]";

    /// <summary>JSON array of AI-suggested scene tags (distinct from user-applied tags in PhotoTag).</summary>
    public string SuggestedTagsJson { get; set; } = "[]";

    /// <summary>Full raw JSON response from the vision API, kept for auditing/reprocessing.</summary>
    public string RawResponseJson { get; set; } = default!;
}
