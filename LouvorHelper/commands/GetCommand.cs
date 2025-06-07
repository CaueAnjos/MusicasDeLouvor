using System.CommandLine;
using LouvorHelper.Models;
using LouvorHelper.Models.Providers;
using LouvorHelper.Utils;

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

        AddOption(titleOption);
        AddOption(authorOption);

        this.SetHandler(CommandAction, titleOption, authorOption);
    }

    private async Task CommandAction(string title, string author)
    {
        Notify.Info($"Buscando letra para: {title} {"de " + author}");

        // TODO: add more providers: Letras.Mus.br
        List<IProvider> providers = [new VagalumeProvider(), new CifraClubProvider()];

        List<Task<string?>> tasks =
        [
            providers[0].GetLyrics(title, author),
            providers[1].GetLyrics(title, author),
        ];

        string? lyrics = (await Task.WhenAll(tasks)).FirstOrDefault(t => t is not null);

        if (lyrics is not null)
        {
            Notify.Success("Letra encontrada");

            Music music = new(title, author, lyrics);
            FileManager fileManager = new();
            await fileManager.SaveAsync(music);

            Notify.Success($"Arquivo salvo em: {fileManager.Path}");
        }
        else
            Notify.Error("Letra não encontrada");
    }
}
