namespace louvorHelper.Models;

internal interface IProvider
{
    Task<string?> GetLyrics(string title, string artist);
}
