namespace LouvorHelper.Models;

internal class PresentationCompiler
{
    public List<Music> Musics { get; private set; }

    public PresentationCompiler(IEnumerable<Music> musics)
    {
        Musics = new List<Music>(musics);
    }

    public PresentationCompiler()
    {
        Musics = new List<Music>();
    }

    public void AddMusicToCompiler(Music music)
    {
        Musics.Add(music);
    }

    public void AddMusicToCompiler(IEnumerable<Music> musics)
    {
        Musics.AddRange(musics);
    }

    public void Compile()
    {
        Notify.Info("Compilando...");
    }
}
