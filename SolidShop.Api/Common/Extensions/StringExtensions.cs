using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SolidShop.Api.Common.Extensions;

// Open/Closed: string is "closed" (we can't modify it) but we can extend it.
public static partial class StringExtensions
{
    // "Zapatilla Running Niño" -> "zapatilla-running-nino"
    public static string ToSlug(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            // Removes accents: "ñ" -> "n", "á" -> "a"
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return NonAlphanumeric().Replace(sb.ToString(), "-").Trim('-');
    }

    // Used for short descriptions in product listings
    public static string Truncate(this string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value ?? string.Empty;
        return value[..maxLength].TrimEnd() + "...";
    }

    // " cam-rv-001 " -> "CAM-RV-001"
    public static string NormalizeSku(this string value) => value.Trim().ToUpperInvariant();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
