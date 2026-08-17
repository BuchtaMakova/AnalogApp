namespace AnalogHub.Infrastructure.AI.Gemini;

/// <summary>
/// Gemini's structured-output schema (an OpenAPI-subset, distinct from plain JSON Schema — types
/// are uppercase, nullability is a separate "nullable" flag rather than a type union) so the vision
/// critique response is guaranteed to match
/// <see cref="Application.Common.Interfaces.PhotoCritiqueResult"/>'s shape.
/// </summary>
internal static class PhotoCritiqueGeminiSchema
{
    public const string Schema = """
        {
          "type": "OBJECT",
          "properties": {
            "compositionScore": {
              "type": "INTEGER",
              "description": "Overall composition quality, 1 (weak) to 10 (excellent)."
            },
            "compositionNotes": {
              "type": "STRING",
              "description": "Analysis of framing, rule of thirds, leading lines and distracting elements."
            },
            "lightingNotes": {
              "type": "STRING",
              "description": "Analysis of available/flash light, direction, and highlight/shadow exposure."
            },
            "posingNotes": {
              "type": "STRING",
              "nullable": true,
              "description": "Posing critique if the photo is a portrait; null for non-portrait subjects."
            },
            "recommendations": {
              "type": "ARRAY",
              "items": { "type": "STRING" },
              "description": "Concrete, actionable tips for future shots."
            },
            "suggestedTags": {
              "type": "ARRAY",
              "items": { "type": "STRING" },
              "description": "Short lowercase scene/subject tags, e.g. 'portrait', 'golden-hour', 'street'."
            }
          },
          "required": [
            "compositionScore",
            "compositionNotes",
            "lightingNotes",
            "posingNotes",
            "recommendations",
            "suggestedTags"
          ]
        }
        """;
}
