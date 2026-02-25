using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyIndustry.ApplicationService.Helpers;

/// <summary>
/// Generates search term variants for flexible matching:
/// Turkish characters (ö, ü, ş, etc.) and common spelling (e.g. kompresor / kompressor).
/// </summary>
public static class SearchTermHelper
{
    private static readonly Dictionary<char, char> TurkishToAscii = new()
    {
        { 'ç', 'c' }, { 'Ç', 'c' },
        { 'ğ', 'g' }, { 'Ğ', 'g' },
        { 'ı', 'i' }, { 'İ', 'i' },
        { 'ö', 'o' }, { 'Ö', 'o' },
        { 'ş', 's' }, { 'Ş', 's' },
        { 'ü', 'u' }, { 'Ü', 'u' }
    };

    private static readonly HashSet<char> Vowels = new() { 'a', 'e', 'ı', 'i', 'o', 'ö', 'u', 'ü' };

    /// <summary>
    /// Returns normalized and spelling variants so that "kompresör", "kompresor", "kompressor" all match.
    /// </summary>
    public static IReadOnlyList<string> GetSearchVariants(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return [];

        var t = term.Trim().ToLowerInvariant();
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { t };

        var normalized = new StringBuilder();
        foreach (var c in t)
            normalized.Append(TurkishToAscii.TryGetValue(c, out var r) ? r : c);
        var norm = normalized.ToString();
        set.Add(norm);

        // "ss" -> "s" so "kompressor" matches when user types "kompresor"
        if (norm.Contains("ss"))
            set.Add(norm.Replace("ss", "s"));

        // "sor" (vowel+s+or) -> "ssor" so "kompressor" matches when user types "kompresör"/"kompresor"
        if (norm.EndsWith("sor") && !norm.EndsWith("ssor") && norm.Length >= 4 &&
            Vowels.Contains(norm[^4]))
            set.Add(norm.Replace("sor", "ssor"));

        return set.ToList();
    }
}
