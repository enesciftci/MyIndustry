using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MyIndustry.ApplicationService.Helpers;

public static class SlugHelper
{
    /// <summary>
    /// Generates a SEO-friendly slug from text
    /// </summary>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Turkish character replacements
        var turkishChars = new Dictionary<char, char>
        {
            { 'ç', 'c' }, { 'Ç', 'C' },
            { 'ğ', 'g' }, { 'Ğ', 'G' },
            { 'ı', 'i' }, { 'İ', 'I' },
            { 'ö', 'o' }, { 'Ö', 'O' },
            { 'ş', 's' }, { 'Ş', 'S' },
            { 'ü', 'u' }, { 'Ü', 'U' }
        };

        var slug = new StringBuilder();
        foreach (var c in text)
        {
            if (turkishChars.ContainsKey(c))
                slug.Append(turkishChars[c]);
            else
                slug.Append(c);
        }

        // Convert to lowercase
        var result = slug.ToString().ToLowerInvariant();

        // Replace spaces and special characters with hyphens
        result = Regex.Replace(result, @"[^a-z0-9\s-]", "");
        result = Regex.Replace(result, @"\s+", "-");
        result = Regex.Replace(result, @"-+", "-");
        result = result.Trim('-');

        // Limit length to 100 characters
        if (result.Length > 100)
            result = result.Substring(0, 100).TrimEnd('-');

        return result;
    }

    /// <summary>
    /// Generates a unique slug by appending a number if the slug already exists
    /// </summary>
    public static async Task<string> GenerateUniqueSlugAsync(
        string baseSlug,
        Func<string, Task<bool>> slugExistsAsync)
    {
        if (string.IsNullOrWhiteSpace(baseSlug))
            return string.Empty;

        var slug = baseSlug;
        var counter = 1;

        while (await slugExistsAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
