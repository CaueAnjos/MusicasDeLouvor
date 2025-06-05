using System.Text.RegularExpressions;
using LouvorHelper.Utils;

namespace LouvorHelper.Models;

internal class CifraClubProvider : IProvider
{
    public async Task<string?> GetLyrics(string title, string artist)
    {
        HttpClient client = new();
        try
        {
            string url = $"https://www.cifraclub.com.br/{PrepareString(artist)}/{PrepareString(title)}.html";
            string text = await client.GetStringAsync(url);

            var match = Regex.Match(text, @"<pre>(.*?)</pre>", RegexOptions.Singleline);
            if (!match.Success)
                return null;

            string rawLyrics = match.Groups[1].Value;

            rawLyrics = Regex.Replace(rawLyrics, @"<b>(.*)</b>", "");
            rawLyrics = Regex.Replace(rawLyrics, @"[\[\(](.*)[\]\)]", "");
            rawLyrics = Regex.Replace(rawLyrics, @"\n{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

            string cleanLyrics = rawLyrics.Trim();
            return cleanLyrics;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
