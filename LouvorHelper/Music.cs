namespace LouvorHelper;

class Music
{
    public Music(string titulo, string artista, string letra)
    {
        Titulo = titulo;
        Artista = artista;
        Letra = letra;
    }

    public string Titulo { get; set; }
    public string Artista { get; set; }
    public string Letra { get; set; }
}
