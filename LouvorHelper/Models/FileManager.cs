using System.Text.Json;

namespace LouvorHelper.Models;

internal class FileManager
{
    public string Path { get; private set; } = System.IO.Path.GetFullPath("../Musicas");

    public async Task SaveAsync(Music music)
    {
        // FIXME: use a better way to format the title!
        string formattedTitle = music.Titulo.ToUpper().Replace(' ', '_').Trim();
        string formattedArtist = music.Artista.ToUpper().Replace(' ', '_').Trim();
        string filePath = System.IO.Path.Combine(Path, $"{formattedTitle}-{formattedArtist}.json");

        JsonSerializerOptions options = new();
        options.WriteIndented = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;

        string json = JsonSerializer.Serialize(music, options);

        Directory.CreateDirectory(Path);
        await File.WriteAllTextAsync(filePath, json);
    }
}
