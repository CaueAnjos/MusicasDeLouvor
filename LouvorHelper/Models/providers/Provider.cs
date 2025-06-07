namespace LouvorHelper.Models.Providers;

internal interface IProvider
{
    Task<string?> GetLyrics(string title, string artist);
}
