using System.Text.Json;

namespace LouvorHelper.Models;

internal class FileManager
{
    private string path = "../Musicas";

    public async Task Save(Music music)
    {
        string formattedTitle = music.Titulo.ToUpper().Replace(' ', '_').Trim();
        string formattedArtist = music.Titulo.ToUpper().Replace(' ', '_').Trim();
        string filePath = Path.Combine(path, $"{formattedTitle}-{formattedArtist}.json");

        JsonSerializerOptions options = new();
        options.WriteIndented = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;

        string json = JsonSerializer.Serialize(music, options);

        Directory.CreateDirectory(path);
        await File.WriteAllTextAsync(filePath, json);
    }
}
