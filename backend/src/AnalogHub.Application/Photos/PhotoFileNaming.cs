using AnalogHub.Domain.Entities;

namespace AnalogHub.Application.Photos;

/// <summary>Builds human-friendly download file names ("KodakPortra400_frame12.jpg") from a photo's roll and frame number.</summary>
public static class PhotoFileNaming
{
    /// <param name="usedNames">
    /// Tracks names already handed out in the current batch (e.g. a ZIP) so two photos from the
    /// same roll never collide; pass null for a single, standalone download.
    /// </param>
    public static string BuildDownloadFileName(Photo photo, HashSet<string>? usedNames = null)
    {
        var extension = GetExtension(photo.ContentType, photo.OriginalStorageKey);
        var rollPart = Sanitize(photo.FilmRoll?.Name);
        var framePart = photo.FrameNumber is { } frame ? $"_frame{frame}" : "";
        var baseName = $"{rollPart}{framePart}{extension}";

        if (usedNames is null)
        {
            return baseName;
        }

        var candidate = baseName;
        var suffix = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = $"{rollPart}{framePart}_{suffix}{extension}";
            suffix++;
        }

        return candidate;
    }

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "photo";
        }

        var cleaned = new string(value.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
        return cleaned.Length == 0 ? "photo" : cleaned;
    }

    private static string GetExtension(string contentType, string storageKey) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/tiff" => ".tiff",
        "image/webp" => ".webp",
        _ => Path.GetExtension(storageKey) is { Length: > 0 } ext ? ext : ".jpg"
    };
}
