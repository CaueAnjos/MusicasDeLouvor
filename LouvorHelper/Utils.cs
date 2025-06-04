using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LouvorHelper;

class Utils
{
    public static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    public enum UrlStyle
    {
        Vagalume,
    }

    public static string PrepareString(string? str, UrlStyle style, string? defaultValue = null)
    {
        if (defaultValue is null && str is null)
            throw new ArgumentNullException(nameof(str));

        return RemoveDiacritics(str?.ToLower().Replace(' ', '-') ?? "diversos");
    }

    public static string? GetLyrics(string text, UrlStyle style)
    {
        var match = Regex.Match(text, @"<div id=lyrics[^>]*>(.*?)</div>", RegexOptions.Singleline);
        if (!match.Success)
            return null;

        string rawLyrics = match.Groups[1].Value;

        rawLyrics = Regex.Replace(rawLyrics, @"<div[^>]*>.*?</div>", "", RegexOptions.Singleline);
        rawLyrics = Regex.Replace(rawLyrics, @"<[^>]+>", "\n"); // <br> → \n

        rawLyrics = Regex.Replace(rawLyrics, @"\n{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

        string cleanLyrics = rawLyrics.Trim();
        return cleanLyrics;
    }
}
