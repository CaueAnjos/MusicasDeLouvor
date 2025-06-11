using System.Text;
using System.Text.Json;

namespace LouvorHelper.Models;

internal class FileManager
{
    public FileManager()
    {
        DownloadPath = Path.GetFullPath("../Musics");
        CompileOutputPath = Path.GetFullPath("../Presentations");

        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.WriteIndented = true;
        _jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
    }

    public string DownloadPath { get; set; }
    public string CompileOutputPath { get; set; }
    private JsonSerializerOptions _jsonOptions;

    public async Task SaveAsync(Music music)
    {
        string formattedTitle = music.Title.ToUpper().Replace(' ', '_').Trim();
        string formattedArtist = music.Artist.ToLower().Replace(' ', '_').Trim();
        string filePath = Path.Combine(DownloadPath, $"{formattedTitle}-{formattedArtist}.json");

        string json = JsonSerializer.Serialize(music, _jsonOptions);

        Directory.CreateDirectory(DownloadPath);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Music?> LoadAsync(string filePath)
    {
        if (Path.Exists(filePath))
        {
            return JsonSerializer.Deserialize<Music>(
                await File.ReadAllTextAsync(filePath, Encoding.UTF8),
                _jsonOptions
            );
        }
        return null;
    }

    public async IAsyncEnumerable<Music> LoadAsync()
    {
        if (!Directory.Exists(DownloadPath))
            yield break;

        foreach (string fileName in Directory.EnumerateFiles(DownloadPath))
        {
            Music? music = await LoadAsync(fileName);

            if (music is not null)
                yield return music;
        }
    }
}
