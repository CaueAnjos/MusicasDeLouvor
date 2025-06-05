using System.Text.RegularExpressions;
using louvorHelper.Models;
using LouvorHelper.Utils;

namespace LouvorHelper.Models;

internal class VagalumeProvider : IProvider
{
    public async Task<string?> GetLyrics(string title, string artist)
    {
        HttpClient client = new();
        string text = await client.GetStringAsync($"https://www.vagalume.com.br/{artist}/{title}.html");

        var match = Regex.Match(text, @"<div id=lyrics[^>]*>(.*?)</div>", RegexOptions.Singleline);
        if (!match.Success)
            return null;

        string rawLyrics = match.Groups[1].Value;

        // rawLyrics = Regex.Replace(rawLyrics, @"<div[^>]*>.*?</div>", "", RegexOptions.Singleline);
        rawLyrics = Regex.Replace(rawLyrics, @"<[^>]+>", "\n"); // <br> → \n
        rawLyrics = Regex.Replace(rawLyrics, @"\n{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

        string cleanLyrics = rawLyrics.Trim();
        return cleanLyrics;
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
