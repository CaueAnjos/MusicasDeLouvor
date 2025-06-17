namespace LouvorHelperCore.Models.Providers;

// TODO: create a proper formatter class for formatting lyrics
public abstract class Provider
{
    protected Provider(string url, string label)
    {
        Url = url;
        Label = label;
    }

    public string Url { get; protected set; }
    public string Label { get; }

    protected abstract string BuildUrl(string title, string artist);

    public virtual async Task<string?> GetLyricsAsync(string title, string artist)
    {
        HttpClient client = new();
        try
        {
            string url = BuildUrl(title, artist);
            string rawLyrics = await client.GetStringAsync(url);
            return rawLyrics;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
