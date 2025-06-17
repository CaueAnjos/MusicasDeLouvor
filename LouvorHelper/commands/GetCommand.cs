using System.CommandLine;
using LouvorHelperCore.Models;
using LouvorHelperCore.Models.Providers;
using LouvorHelperCore.Utils;

namespace LouvorHelper.Commands;

internal class GetCommand : Command
{
    public GetCommand()
        : base("get", "Faz o download dos arquivos de músicas de Louvor")
    {
        var titleOption = new Option<string>(["--title", "--titulo", "-t"], "O título da música")
        {
            IsRequired = true,
        };
        var authorOption = new Option<string>(["--author", "--autor", "-a"], "O nome do autor")
        {
            IsRequired = true,
        };
        var autoCompileOption = new Option<bool>(
            ["--auto-compile", "--compilar", "-c"],
            getDefaultValue: () => true,
            "Compila automaticamente as apresentações"
        );

        AddOption(titleOption);
        AddOption(authorOption);
        AddOption(autoCompileOption);

        this.SetHandler(CommandAction, titleOption, authorOption, autoCompileOption);
    }

    private async Task CommandAction(string title, string author, bool autoCompile)
    {
        Notify.Info($"Buscando letra para: {title} {"de " + author}");

        ProviderContainer providerContainer = new(
            [new VagalumeProvider(), new CifraClubProvider(), new LetrasMusProvider()]
        );
        await providerContainer.GetLyricsAsync(title, author);

        string? lyrics = SelectProvider(providerContainer);

        if (lyrics is not null)
        {
            Notify.Success("Letra encontrada");

            Music music = new(title, author, lyrics);
            FileManager fileManager = new();
            await fileManager.SaveAsync(music);

            Notify.Success($"Arquivo salvo em: {fileManager.DownloadPath}");

            if (autoCompile)
            {
                PresentationCompiler compiler = new();
                compiler.CompileMusic(music);
                Notify.Success($"{music.Title} was compiled");
            }
        }
        else
            Notify.Error("Letra não encontrada");
    }

    private string? SelectProvider(ProviderContainer container)
    {
        if (container.GoodProvidersResponse == 0)
            return null;

        if (container.GoodProvidersResponse == 1)
            return container.GetDefaultLyrics();

        foreach (var result in container.Lyrics)
        {
            if (result.Value is null)
                Notify.Info($"{result.Key}");
            else
                Notify.Success($"{result.Key}");
        }

        Notify.Info("Qual você deseja usar?");
        string? choice = Console.ReadLine();

        if (!string.IsNullOrEmpty(choice) && container.Lyrics.ContainsKey(choice))
            return container.Lyrics[choice];
        else
            return container.GetDefaultLyrics();
    }
}
