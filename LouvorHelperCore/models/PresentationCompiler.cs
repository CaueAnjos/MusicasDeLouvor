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

    public async Task CompileAllAsync()
    {
        Notify.Info("Iniciando compilação...");

        FileManager.ClearCompiled();

        List<Task> tasks = new();
        int errorCount = 0;
        await foreach (Music music in FileManager.LoadAsync())
        {
            Notify.Info($"Compilando {music.Title} - {music.Artist}");

            Musics.Enqueue(music);
            try
            {
                string filePath = Path.Combine(
                    FileManager.CompileOutputPath,
                    FileManager.ApropriateFileName(music)
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        PresentationDocument presentation = new(music);
                        presentation.SetTemplate(TemplatePath);
                        presentation.Save(filePath);
                        Notify.Success($"Compilado com sucesso: {Path.GetFileName(filePath)}");
                    })
                );
            }
            catch (NullReferenceException)
            {
                Notify.Error($"Erro: json não foi formatado corratamente");
                errorCount++;
            }
        }
        await Task.WhenAll(tasks);

        if (errorCount > 0)
            Notify.Warning($"Ocorreram {errorCount} erros ao compilar apresentações");

        if (errorCount < Musics.Count)
            Notify.Success(
                $"Compilação concluída! {Musics.Count - errorCount} apresentações criadas em '{FileManager.CompileOutputPath}'"
            );
    }
}
