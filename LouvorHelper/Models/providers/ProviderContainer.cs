namespace LouvorHelper.Models.Providers;

internal class ProviderContainer
{
    private List<IProvider> _providers;
    public Dictionary<string, string?> Lyrics { get; private set; }

    public ProviderContainer(List<IProvider> providers)
    {
        _providers = providers;
        Lyrics = new();
    }

    public async Task GetLyricsAsync(string title, string artist)
    {
        List<Task<KeyValuePair<string, string?>>> tasks = new();
        foreach (IProvider provider in _providers)
            tasks.Add(provider.GetLyrics(title, artist));

        Lyrics = new(await Task.WhenAll(tasks));
    }

    public string? GetDefaultLyrics()
    {
        return Lyrics.FirstOrDefault(t => t.Value is not null).Value;
    }

    public int GoodProvidersResponse => Lyrics.Count(t => t.Value is not null);
}
