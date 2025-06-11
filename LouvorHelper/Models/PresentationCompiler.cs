using LouvorHelper.Models.Presentation;
using LouvorHelper.Utils;

namespace LouvorHelper.Models;

internal class PresentationCompiler
{
    public Queue<Music> Musics { get; private set; }
    public FileManager FileManager { get; private set; }
    public string TemplatePath { get; set; } = string.Empty;

    public PresentationCompiler(IEnumerable<Music> musics, FileManager? fileManager = null)
    {
        Musics = new Queue<Music>(musics);
        FileManager = fileManager is null ? new FileManager() : fileManager;
    }

    public PresentationCompiler(FileManager? fileManager = null)
    {
        Musics = new Queue<Music>();
        FileManager = fileManager is null ? new FileManager() : fileManager;
    }

    public void AddMusicToCompiler(Music music)
    {
        Musics.Enqueue(music);
    }

    public void AddMusicToCompiler(IEnumerable<Music> musics)
    {
        foreach (Music music in musics)
            Musics.Enqueue(music);
    }

    public async Task CompileAllAsync()
    {
        Notify.Info("Iniciando compilação...");

        FileManager.ClearCompiled();

        List<Task> tasks = new();
        await foreach (Music music in FileManager.LoadAsync())
        {
            Notify.Info($"Compilando {music.Title} - {music.Artist}");

            Musics.Enqueue(music);
            try
            {
                string titleFormatted = music.Title.ToUpper().Trim().Replace(' ', '_');
                string artistFormatted = music.Artist.ToLower().Trim().Replace(' ', '_');
                string fileName = SanitizeFileName($"{titleFormatted}-{artistFormatted}.pptx");
                string filePath = Path.Combine(FileManager.CompileOutputPath, fileName);
                tasks.Add(
                    Task.Run(() =>
                    {
                        PresentationDocument presentation = new(music);
                        presentation.SetTemplate(TemplatePath);
                        presentation.Save(filePath);
                        Notify.Success($"Compilado com sucesso: {fileName}");
                    })
                );
            }
            catch (NullReferenceException)
            {
                Notify.Error($"Erro: json não foi formatado corratamente");
            }
        }
        await Task.WhenAll(tasks);

        Notify.Success(
            $"Compilação concluída! {Musics.Count} apresentações criadas em '{FileManager.CompileOutputPath}'"
        );
    }

    /// <summary>
    /// Sanitizes filename to remove invalid characters
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "_",
            fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );
        return sanitized.Length > 100 ? sanitized.Substring(0, 100) + ".pptx" : sanitized;
    }
}
