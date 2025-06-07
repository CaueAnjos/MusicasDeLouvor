using System.CommandLine;
using LouvorHelper.Models;

namespace LouvorHelper.Commands;

internal class CompileCommand : Command
{
    public CompileCommand()
        : base("compile", "Compila os arquivos de músicas de Louvor")
    {
        this.SetHandler(CommandAction);
    }

    private async Task CommandAction()
    {
        PresentationCompiler compiler = new();
        await compiler.CompileAllAsync();
    }
}
