namespace AnalogHub.Application.Photos.Dtos;

public sealed record PhotoCritiqueDto(
    Guid PhotoId,
    string Model,
    int CompositionScore,
    string CompositionNotes,
    string LightingNotes,
    string? PosingNotes,
    IReadOnlyList<string> Recommendations,
    IReadOnlyList<string> SuggestedTags,
    DateTimeOffset CreatedAtUtc);

public sealed record AnalyzePhotoResult(PhotoCritiqueDto Critique, IReadOnlyList<string> AppliedTags);
