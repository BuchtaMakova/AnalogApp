namespace AnalogHub.Application.Tags.Dtos;

public sealed record TagDto(Guid Id, string Name, string Slug);

public sealed record PhotoTagDto(Guid TagId, string Name, string Slug, bool IsAiSuggested);
