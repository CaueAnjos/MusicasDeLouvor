namespace LouvorHelper.Models.Providers;

internal interface IProvider
{
    // KeyValuePair<string? Lyrics, string Lable>
    Task<KeyValuePair<string, string?>> GetLyrics(string title, string artist);
    string Label { get; }
}
