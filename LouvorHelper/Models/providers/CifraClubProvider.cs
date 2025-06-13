using System.Text.RegularExpressions;
using LouvorHelper.Utils;

namespace LouvorHelper.Models.Providers;

internal class CifraClubProvider : IProvider
{
    public string Label { get; } = "CifraClub";

    public async Task<KeyValuePair<string, string?>> GetLyrics(string title, string artist)
    {
        HttpClient client = new();
        try
        {
            string url =
                $"https://www.cifraclub.com.br/{PrepareString(artist)}/{PrepareString(title)}/";
            string text = await client.GetStringAsync(url);

            var match = Regex.Match(text, @"<pre>(.*?)</pre>", RegexOptions.Singleline);
            if (!match.Success)
                return ProviderReturnPair.ReturnPair(Label);

            string rawLyrics = match.Groups[1].Value;
            rawLyrics = Regex.Replace(rawLyrics, @"<.*>", "");
            rawLyrics = Regex.Replace(rawLyrics, @"[A-Z].*\|", "");
            rawLyrics = Regex.Replace(rawLyrics, @"Parte [0-9]* de [0-9]*", "");
            rawLyrics = Regex.Replace(rawLyrics, @"[\[\(](.*)[\]\)]", "");
            rawLyrics = Regex.Replace(rawLyrics, @"[-_]{2,}", "");
            rawLyrics = Regex.Replace(rawLyrics, @"[\n ]{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

            string cleanLyrics = rawLyrics.Trim();
            return ProviderReturnPair.ReturnPair(Label, cleanLyrics);
        }
        catch (HttpRequestException)
        {
            return ProviderReturnPair.ReturnPair(Label);
        }
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
