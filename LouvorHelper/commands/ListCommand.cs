using System.CommandLine;
using LouvorHelperCore.Models;
using LouvorHelperCore.Utils;

namespace LouvorHelper.Commands;

internal class ListCommand : Command
{
    public ListCommand()
        : base("list", "Lista as músicas de Louvor")
    {
        Option<bool> enumerateOption = new(["--enumerate", "-e"], "Enumera todas as músicas");
        AddOption(enumerateOption);
        this.SetHandler(CommandAction, enumerateOption);
    }

    private async Task CommandAction(bool enumerate)
    {
        FileManager fileManager = new();
        int count = 1;

        await foreach (Music music in fileManager.LoadAsync())
        {
            Notify.Info(enumerate ? GetMessage(count, music) : GetMessage(music));
            count++;
        }
    }

    private string GetMessage(int index, Music music)
    {
        return $"({index}) {music.Title} - {music.Artist}";
    }

    private string GetMessage(Music music)
    {
        return $"{music.Title} - {music.Artist}";
    }
}
