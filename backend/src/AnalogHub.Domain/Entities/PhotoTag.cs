namespace AnalogHub.Domain.Entities;

/// <summary>Explicit join entity (not a skip-navigation) so we can flag whether a tag came from the AI critique.</summary>
public sealed class PhotoTag
{
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = default!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = default!;

    public bool IsAiSuggested { get; set; }
}
