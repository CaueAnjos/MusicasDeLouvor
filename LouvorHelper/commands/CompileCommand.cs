using System.CommandLine;
using LouvorHelper.Models;

namespace LouvorHelper.Commands;

internal class CompileCommand : Command
{
    public CompileCommand()
        : base("compile", "Compila os arquivos de músicas de Louvor")
    {
        Option<string?> templateOption = new(
            ["--template", "-m"],
            "O caminho para o modelo que será aplicado as apresentações"
        );
        AddOption(templateOption);
        this.SetHandler(CommandAction, templateOption);
    }

    private async Task CommandAction(string? templatePath)
    {
        PresentationCompiler compiler = new();
        if (templatePath is not null)
            compiler.TemplatePath = templatePath;
        await compiler.CompileAllAsync();
    }
}
