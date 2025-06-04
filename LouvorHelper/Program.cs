using System.CommandLine;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LouvorHelper;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Ajuda a fazer o download dos arquivos de músicas de Louvor");

        var tituloOption = new Option<string>(["--titulo", "-t"], "O título da música") { IsRequired = true };
        var autorOption = new Option<string?>(["--autor", "-a"], "O nome do autor");

        var getCommand = new Command("get", "Faz o download dos arquivos de músicas de Louvor");
        getCommand.AddOption(tituloOption);
        getCommand.AddOption(autorOption);

        getCommand.SetHandler(async (string titulo, string? autor) =>
        {
            await GetCommand_Handler(titulo, autor);
        }, tituloOption, autorOption);

        rootCommand.AddCommand(getCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task GetCommand_Handler(string titulo, string? autor)
    {
        string artista = Utils.PrepareString(autor, Utils.UrlStyle.Vagalume, "diversos");
        string musica = Utils.PrepareString(titulo, Utils.UrlStyle.Vagalume);
        Console.WriteLine($"Buscando letra para: {musica} {(autor != null ? "de " + artista : "")}");

        string url = $"https://www.vagalume.com.br/{artista}/{musica}.html";

        using HttpClient client = new();
        try
        {
            string html = await client.GetStringAsync(url);

            string? lyrics = Utils.GetLyrics(html, Utils.UrlStyle.Vagalume);
            if (lyrics is not null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Letra encontrada");
                var music = new Music(musica, artista, lyrics);

                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;

                string json = JsonSerializer.Serialize(music, options);
                Directory.CreateDirectory("../Musicas");
                File.WriteAllText($"../Musicas/{music.Titulo}_{music.Artista}.json", json);
            }
            else
                Console.WriteLine("Letra não encontrada.");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("Erro ao buscar a letra. Verifique o título e o autor.");
        }
    }
}
