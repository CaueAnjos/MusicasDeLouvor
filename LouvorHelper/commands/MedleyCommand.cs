using System.CommandLine;
using System.Text;
using LouvorHelperCore.Models;
using LouvorHelperCore.Utils;

namespace LouvorHelper.Commands;

internal class MedleyCommand : Command
{
    public MedleyCommand()
        : base("medley", "Cria um medley das musicas especificadas")
    {
        Option<List<FileInfo>> filesForMedleyOption = new(
            ["--files-for-medley", "--files", "-f"],
            "Os de musica (.json) para o medley"
        )
        {
            IsRequired = true,
        };

        Option<string?> authorOption = new(
            ["--author", "--autor", "-a"],
            "define o autor do medley"
        );
        AddOption(filesForMedleyOption);
        this.SetHandler(CommandAction, filesForMedleyOption, authorOption);
    }

    bool NotifyErrorAndConfirm(string message)
    {
        Notify.Error(message);
        return Notify.YesNoBox("Deseja continuar com outro arquivo?", true);
    }

    private async Task CommandAction(List<FileInfo> filesForMedley, string? author)
    {
        if (filesForMedley.Count <= 1)
        {
            Notify.Error("Nenhum arquivo foi especificado");
            return;
        }

        FileManager fileManager = new();
        author ??= "LouvorHelper";

        var lyricsBuilder = new StringBuilder();
        var titleBuilder = new StringBuilder("(Medley) ");

        foreach (FileInfo file in filesForMedley)
        {
            if (file.Extension != ".json")
            {
                if (!NotifyErrorAndConfirm($"O arquivo {file.Name} não é um arquivo JSON"))
                    return;
                continue;
            }

            if (file.Length == 0)
            {
                if (!NotifyErrorAndConfirm($"{file.Name} está vazio"))
                    return;
                continue;
            }

            Music? musicFromFile = await fileManager.LoadAsync(file.FullName);
            if (musicFromFile is null)
            {
                if (
                    !NotifyErrorAndConfirm(
                        $"Não foi possível carregar o arquivo {file.Name}. Talvez ele não contenha uma música no formato válido."
                    )
                )
                    return;
                continue;
            }

            lyricsBuilder.AppendLine(musicFromFile.Lyrics).AppendLine();
            titleBuilder.Append(musicFromFile.Title).Append('+');
        }

        if (lyricsBuilder.Length == 0 || titleBuilder.Length <= "(Medley) ".Length)
        {
            Notify.Error("Nenhum arquivo válido foi processado para criar o medley.");
            return;
        }

        string finalTitle = titleBuilder.ToString().TrimEnd('+');
        string finalLyrics = lyricsBuilder.ToString().Trim();

        Music medley = new(finalTitle, author, finalLyrics);
        await fileManager.SaveAsync(medley);
        Notify.Success($"Arquivo salvo em: {fileManager.DownloadPath}");
    }
}
