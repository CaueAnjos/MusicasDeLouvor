using System.Text;
using System.Text.Json;

namespace LouvorHelper.Models;

internal class FileManager
{
    public FileManager()
    {
        DownloadPath = Path.GetFullPath("../Musicas");
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
        // FIXME: use a better way to format the title!
        string formattedTitle = music.Title.ToUpper().Replace(' ', '_').Trim();
        string formattedArtist = music.Artist.ToUpper().Replace(' ', '_').Trim();

        string json = JsonSerializer.Serialize(music, _jsonOptions);

        Directory.CreateDirectory(DownloadPath);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async IAsyncEnumerable<Music> LoadAsync()
    {
        if (!Directory.Exists(DownloadPath))
            yield break;

        foreach (string fileName in Directory.EnumerateFiles(DownloadPath))
        {
            string json = await File.ReadAllTextAsync(fileName, Encoding.UTF8);

            Music? music = JsonSerializer.Deserialize<Music>(json, _jsonOptions);

            if (music is not null)
                yield return music;
        }
    }
}
