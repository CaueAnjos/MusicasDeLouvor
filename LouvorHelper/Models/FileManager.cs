using System.Text;
using System.Text.Json;

namespace LouvorHelper.Models;

internal class FileManager
{
    public string Path { get; set; } = System.IO.Path.GetFullPath("../Musicas");
    public string CompileOutputPath { get; set; } = System.IO.Path.GetFullPath("../Presentations");

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

    public async IAsyncEnumerable<Music> LoadAsync()
    {
        if (!Directory.Exists(Path))
            yield break;

        foreach (string fileName in Directory.EnumerateFiles(Path))
        {
            string json = await File.ReadAllTextAsync(fileName, Encoding.UTF8);

            JsonSerializerOptions options = new();
            options.WriteIndented = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
            Music? music = JsonSerializer.Deserialize<Music>(json, options);

            if (music is not null)
                yield return music;
        }
    }
}
