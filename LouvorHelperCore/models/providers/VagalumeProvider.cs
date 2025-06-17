using System.Text.RegularExpressions;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models.Providers;

public class VagalumeProvider : IProvider
{
    public string Label { get; } = "Vagalume";

    public async Task<KeyValuePair<string, string?>> GetLyrics(string title, string artist)
    {
        HttpClient client = new();
        try
        {
            string url =
                $"https://www.vagalume.com.br/{PrepareString(artist)}/{PrepareString(title)}.html";
            string text = await client.GetStringAsync(url);

            var match = Regex.Match(
                text,
                @"<div id=lyrics[^>]*>(.*?)</div>",
                RegexOptions.Singleline
            );
            if (!match.Success)
                return ProviderReturnPair.ReturnPair(Label);

            string rawLyrics = match.Groups[1].Value;

            rawLyrics = Regex.Replace(rawLyrics, @"<[^>]+>", "\n"); // <br> → \n
            rawLyrics = Regex.Replace(rawLyrics, @"\n{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

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
