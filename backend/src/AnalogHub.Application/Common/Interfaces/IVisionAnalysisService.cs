namespace AnalogHub.Application.Common.Interfaces;

/// <summary>Multimodal vision analysis over a scanned photo (GPT-4o / Claude Vision-class model).</summary>
public interface IVisionAnalysisService
{
    Task<PhotoCritiqueResult> AnalyzePhotoAsync(
        string imageUrl,
        VisionAnalysisContext context,
        CancellationToken cancellationToken);
}

/// <summary>Gear/film metadata handed to the model so the critique can reference actual shooting conditions.</summary>
public sealed record VisionAnalysisContext(
    string? CameraModel,
    string? LensModel,
    string? FilmStock,
    int? IsoUsed,
    bool? FlashFired);

/// <summary>
/// Guaranteed-shape structured output of a vision critique pass. Provider implementations enforce
/// this shape via a strict JSON schema on the model response, not by best-effort parsing.
/// </summary>
public sealed record PhotoCritiqueResult(
    string Model,
    int CompositionScore,
    string CompositionNotes,
    string LightingNotes,
    string? PosingNotes,
    IReadOnlyList<string> Recommendations,
    IReadOnlyList<string> SuggestedTags,
    string RawResponseJson);
