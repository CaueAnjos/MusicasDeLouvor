namespace LouvorHelper.Models.Providers;

// TODO: create a proper formatter class for formatting lyrics
internal interface IProvider
{
    // KeyValuePair<string? Lyrics, string Lable>
    Task<KeyValuePair<string, string?>> GetLyrics(string title, string artist);
    string Label { get; }
}
