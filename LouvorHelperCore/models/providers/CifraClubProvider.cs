using System.Text;
using System.Text.RegularExpressions;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models.Providers;

public class CifraClubProvider : Provider
{
    public CifraClubProvider()
        : base(label: "CifraClub", url: "https://www.cifraclub.com.br/") { }

    public override async Task<string?> GetLyricsAsync(string title, string artist)
    {
        string? text = await base.GetLyricsAsync(title, artist);
        if (text is null)
            return null;

        var match = Regex.Match(text, @"<pre>(.*?)</pre>", RegexOptions.Singleline);
        if (!match.Success)
            return null;

        string rawLyrics = match.Groups[1].Value;
        rawLyrics = Regex.Replace(rawLyrics, @"<.*>", "");
        rawLyrics = Regex.Replace(rawLyrics, @"[A-Z].*\|", "");
        rawLyrics = Regex.Replace(rawLyrics, @"Parte [0-9]* de [0-9]*", "");
        rawLyrics = Regex.Replace(rawLyrics, @"[\[\(](.*)[\]\)]", "");
        rawLyrics = Regex.Replace(rawLyrics, @"[-_]{2,}", "");
        rawLyrics = Regex.Replace(rawLyrics, @"[\n ]{2,}", "\n\n"); // Múltiplas quebras → 2 quebras

        string cleanLyrics = rawLyrics.Trim();
        return cleanLyrics;
    }

    protected override string BuildUrl(string title, string artist)
    {
        StringBuilder url = new(Url);
        url.Append(PrepareString(artist));
        url.Append('/');
        url.Append(PrepareString(title));
        url.Append('/');
        return url.ToString();
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
