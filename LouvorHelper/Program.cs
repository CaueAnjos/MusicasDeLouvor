﻿using System.CommandLine;
using LouvorHelper.Models;

namespace LouvorHelper;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Ajuda a fazer o download dos arquivos de músicas de Louvor");

        var tituloOption = new Option<string>(["--titulo", "-t"], "O título da música") { IsRequired = true };
        var autorOption = new Option<string>(["--autor", "-a"], "O nome do autor") { IsRequired = true };

        // TODO: put it in a class
        var getCommand = new Command("get", "Faz o download dos arquivos de músicas de Louvor");
        getCommand.AddOption(tituloOption);
        getCommand.AddOption(autorOption);

        getCommand.SetHandler(async (string titulo, string autor) =>
        {
            await GetCommand_Handler(titulo, autor);
        }, tituloOption, autorOption);

        var compileCommand = new Command("compile", "Compila os arquivos de músicas de Louvor");
        compileCommand.SetHandler(async () =>
        {
            await CompileCommand_Handler();
        });

        rootCommand.AddCommand(getCommand);
        rootCommand.AddCommand(compileCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task GetCommand_Handler(string titulo, string autor)
    {
        Notify.Info($"Buscando letra para: {titulo} {"de " + autor}");

        // TODO: add more providers: Letras.Mus.br
        List<IProvider> providers =
        [
            new VagalumeProvider(),
            new CifraClubProvider()
        ];

        List<Task<string?>> tasks =
        [
            providers[0].GetLyrics(titulo, autor),
            providers[1].GetLyrics(titulo, autor)
        ];

        string? lyrics = (await Task.WhenAll(tasks)).FirstOrDefault(t => t is not null);

        if (lyrics is not null)
        {
            Notify.Success("Letra encontrada");

            Music music = new(titulo, autor, lyrics);
            FileManager fileManager = new();
            await fileManager.SaveAsync(music);

            Notify.Success($"Arquivo salvo em: {fileManager.Path}");
        }
        else
            Notify.Error("Letra não encontrada");
    }

    public static async Task CompileCommand_Handler()
    {
        PresentationCompiler compiler = new();
        await compiler.CompileAllAsync();
    }
}
