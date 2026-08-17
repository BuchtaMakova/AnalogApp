using System.Text;
using System.Text.RegularExpressions;

namespace AnalogHub.Application.Common.Utils;

public static partial class Slugify
{
    public static string Generate(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var withoutDiacritics = RemoveDiacritics(normalized);
        var slug = NonAlphanumericRegex().Replace(withoutDiacritics, "-").Trim('-');
        return CollapseDashesRegex().Replace(slug, "-");
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex CollapseDashesRegex();
}
