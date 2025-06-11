using System.CommandLine;
using LouvorHelper.Commands;

namespace LouvorHelper;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(
            "Ajuda a fazer o download dos arquivos de músicas de Louvor"
        );

        rootCommand.AddCommand(new GetCommand());
        rootCommand.AddCommand(new CompileCommand());
        rootCommand.AddCommand(new MedleyCommand());

        return await rootCommand.InvokeAsync(args);
    }
}
