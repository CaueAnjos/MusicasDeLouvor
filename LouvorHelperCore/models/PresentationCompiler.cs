using LouvorHelperCore.Models.Presentation;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models;

public class PresentationCompiler
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

    public void CompileMusic(Music music)
    {
        string filePath = Path.Combine(
            FileManager.CompileOutputPath,
            FileManager.ApropriateFileName(music, extension: ".pptx")
        );

        PresentationDocument presentation = new(music);
        presentation.SetTemplate(TemplatePath);
        presentation.Save(filePath);
        Notify.Success($"Compilado com sucesso: {Path.GetFileName(filePath)}");
    }

    public async Task CompileQueueAsync()
    {
        FileManager.ClearCompiled();

        int index = 0;
        int maxTasksPerCompileCicle = 5;
        int count = Musics.Count;
        Task[] tasks = new Task[maxTasksPerCompileCicle];

        foreach (Music music in Musics)
        {
            tasks[index] = Task.Run(() =>
            {
                CompileMusic(music);
            });

            if (index >= maxTasksPerCompileCicle || count == 0)
            {
                await Task.WhenAll(tasks);
                index = 0;
            }

            count--;
        }
    }

    public async Task CompileAllAsync()
    {
        await foreach (Music music in FileManager.LoadAsync())
        {
            Notify.Info($"Compilando {music.Title} - {music.Artist}");
            Musics.Enqueue(music);
        }

        await CompileQueueAsync();

        Notify.Success(
            $"Compilação concluída! {Musics.Count} apresentações criadas em '{FileManager.CompileOutputPath}'"
        );
    }
}
