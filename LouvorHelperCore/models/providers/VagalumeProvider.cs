using System.Text;
using System.Text.RegularExpressions;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models.Providers;

public class VagalumeProvider : Provider
{
    public VagalumeProvider()
        : base(label: "Vagalume", url: "https://www.vagalume.com.br/") { }

    public override async Task<string?> GetLyricsAsync(string title, string artist)
    {
        string? text = await base.GetLyricsAsync(title, artist);
        if (text is null)
            return null;

        var match = Regex.Match(text, @"<div id=lyrics[^>]*>(.*?)</div>", RegexOptions.Singleline);

        if (!match.Success)
            return null;

        string rawLyrics = match.Groups[1].Value;

        rawLyrics = Regex.Replace(rawLyrics, @"<[^>]+>", "\n"); // <br> → \n
        rawLyrics = Regex.Replace(rawLyrics, @"\n{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

        string cleanLyrics = rawLyrics.Trim();
        return cleanLyrics;
    }

    protected override string BuildUrl(string title, string artist)
    {
        StringBuilder url = new(Url);
        url.Append(PrepareString(artist));
        url.Append('/');
        url.Append(PrepareString(title));
        url.Append(".html");
        return url.ToString();
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
