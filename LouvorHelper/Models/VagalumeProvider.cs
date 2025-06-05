using System.Text.RegularExpressions;
using LouvorHelper.Utils;

namespace LouvorHelper.Models;

internal class VagalumeProvider : IProvider
{
    public async Task<string?> GetLyrics(string title, string artist)
    {
        HttpClient client = new();
        try
        {

            string url = $"https://www.vagalume.com.br/{PrepareString(artist)}/{PrepareString(title)}.html";
            string text = await client.GetStringAsync(url);

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
