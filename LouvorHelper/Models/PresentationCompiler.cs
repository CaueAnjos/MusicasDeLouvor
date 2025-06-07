using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using P = DocumentFormat.OpenXml.Presentation;
using LouvorHelper.Utils;
using D = DocumentFormat.OpenXml.Drawing;

namespace LouvorHelper.Models;

// FIXME: understand what you are doing here

internal class PresentationCompiler
{
    public List<Music> Musics { get; private set; }
    public FileManager FileManager { get; private set; }

    public PresentationCompiler(IEnumerable<Music> musics, FileManager? fileManager = null)
    {
        Musics = new List<Music>(musics);
        FileManager = fileManager is null ? new FileManager() : fileManager;
    }

    public PresentationCompiler(FileManager? fileManager = null)
    {
        Musics = new List<Music>();
        FileManager = fileManager is null ? new FileManager() : fileManager;
    }

    public void AddMusicToCompiler(Music music)
    {
        Musics.Add(music);
    }

    public void AddMusicToCompiler(IEnumerable<Music> musics)
    {
        Musics.AddRange(musics);
    }

    public async Task CompileAllAsync()
    {
        Notify.Info("Iniciando compilação...");

        if (!Directory.Exists(FileManager.CompileOutputPath))
            Directory.CreateDirectory(FileManager.CompileOutputPath);

        List<Task> tasks = new();
        await foreach (Music music in FileManager.LoadAsync())
        {
            Notify.Info($"Compilando {music.Titulo} - {music.Artista}");

            Musics.Add(music);
            string fileName = SanitizeFileName($"{music.Titulo.ToUpper()}-{music.Artista.ToUpper()}.pptx");
            string filePath = Path.Combine(FileManager.CompileOutputPath, fileName);
            tasks.Add(Task.Run(() =>
            {
                CreatePresentationForMusic(music, filePath);
            }));
        }
        await Task.WhenAll(tasks);

        Notify.Success($"Compilação concluída! {Musics.Count} apresentações criadas em '{FileManager.CompileOutputPath}'");
    }

    /// <summary>
    /// Creates a presentation for a single music
    /// </summary>
    public void CreatePresentationForMusic(Music music, string filepath)
    {
        try
        {
            using PresentationDocument presentationDocument = PresentationDocument.Create(filepath, PresentationDocumentType.Presentation);
            // Add presentation part
            PresentationPart presentationPart = presentationDocument.AddPresentationPart();
            presentationPart.Presentation = new P.Presentation();

            // Create slide master part
            CreateSlideMaster(presentationPart);

            // Create slides for the music
            CreateSlidesForMusic(presentationPart, music);

            // Set slide size (16:9 widescreen)
            presentationPart.Presentation.SlideSize = new P.SlideSize()
            {
                Cx = 12192000, // 16:9 width
                Cy = 6858000,  // 16:9 height
                Type = P.SlideSizeValues.Screen16x9
            };

            presentationPart.Presentation.Save();
        }
        catch (Exception ex)
        {
            Notify.Error($"Erro ao criar apresentação para {music.Titulo}: {ex.Message}");
            throw;
        }
    }

    private void CreateSlideMaster(PresentationPart presentationPart)
    {
        // Create slide master part
        SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>("rId1");
        slideMasterPart.SlideMaster = new P.SlideMaster(
            new P.CommonSlideData(new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new D.TransformGroup())
            )),
            new P.ColorMap()
            {
                Background1 = D.ColorSchemeIndexValues.Light1,
                Text1 = D.ColorSchemeIndexValues.Dark1,
                Background2 = D.ColorSchemeIndexValues.Light2,
                Text2 = D.ColorSchemeIndexValues.Dark2,
                Accent1 = D.ColorSchemeIndexValues.Accent1,
                Accent2 = D.ColorSchemeIndexValues.Accent2,
                Accent3 = D.ColorSchemeIndexValues.Accent3,
                Accent4 = D.ColorSchemeIndexValues.Accent4,
                Accent5 = D.ColorSchemeIndexValues.Accent5,
                Accent6 = D.ColorSchemeIndexValues.Accent6,
                Hyperlink = D.ColorSchemeIndexValues.Hyperlink,
                FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink
            }
        );

        // Create slide layout
        SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>("rId1");
        slideLayoutPart.SlideLayout = new P.SlideLayout(
            new P.CommonSlideData(new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new D.TransformGroup())
            )),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );

        slideMasterPart.SlideMaster.Save();

        // Add SlideMasterId to presentation
        presentationPart.Presentation.SlideMasterIdList = new P.SlideMasterIdList(
            new P.SlideMasterId() { Id = (UInt32Value)2147483648U, RelationshipId = "rId1" }
        );
    }

    private void CreateSlidesForMusic(PresentationPart presentationPart, Music music)
    {
        var slideIdList = new P.SlideIdList();
        uint slideId = 256;
        int relationshipId = 2;

        // Title slide
        SlidePart titleSlidePart = presentationPart.AddNewPart<SlidePart>($"rId{relationshipId}");
        titleSlidePart.Slide = CreateTitleSlide(music.Titulo, music.Artista);
        titleSlidePart.Slide.Save();

        slideIdList.Append(new P.SlideId()
        {
            Id = (UInt32Value)slideId++,
            RelationshipId = $"rId{relationshipId++}"
        });

        // Lyrics slides - split lyrics into verses/choruses
        if (!string.IsNullOrEmpty(music.Letra))
        {
            var lyricSections = SplitLyrics(music.Letra);
            foreach (var section in lyricSections)
            {
                SlidePart lyricSlidePart = presentationPart.AddNewPart<SlidePart>($"rId{relationshipId}");
                lyricSlidePart.Slide = CreateLyricSlide(section);
                lyricSlidePart.Slide.Save();

                slideIdList.Append(new P.SlideId()
                {
                    Id = (UInt32Value)slideId++,
                    RelationshipId = $"rId{relationshipId++}"
                });
            }
        }

        // Add all slides to presentation
        presentationPart.Presentation.SlideIdList = slideIdList;
    }

    private static P.Slide CreateTitleSlide(string title, string artist)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),

                    // Title
                    new P.Shape(
                        new P.NonVisualShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 2, Name = "Title" },
                            new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                            new P.ApplicationNonVisualDrawingProperties(new P.PlaceholderShape() { Type = P.PlaceholderValues.Title })
                        ),
                        new P.ShapeProperties(
                            new D.Transform2D(
                                new D.Offset() { X = 914400L, Y = 1828800L },
                                new D.Extents() { Cx = 10297200L, Cy = 1368296L }
                            )
                        ),
                        new P.TextBody(
                            new D.BodyProperties() { Anchor = D.TextAnchoringTypeValues.Center },
                            new D.ListStyle(),
                            new D.Paragraph(
                                new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                                new D.Run(
                                    new D.RunProperties() { FontSize = 4400, Bold = true },
                                    new D.Text(title)
                                )
                            )
                        )
                    ),

                    // Artist subtitle
                    new P.Shape(
                        new P.NonVisualShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 3, Name = "Subtitle" },
                            new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                            new P.ApplicationNonVisualDrawingProperties()
                        ),
                        new P.ShapeProperties(
                            new D.Transform2D(
                                new D.Offset() { X = 914400L, Y = 3429000L },
                                new D.Extents() { Cx = 10297200L, Cy = 1000000L }
                            )
                        ),
                        new P.TextBody(
                            new D.BodyProperties() { Anchor = D.TextAnchoringTypeValues.Center },
                            new D.ListStyle(),
                            new D.Paragraph(
                                new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                                new D.Run(
                                    new D.RunProperties() { FontSize = 2800 },
                                    new D.Text($"Por: {artist}")
                                )
                            )
                        )
                    )
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );
    }

    private static P.Slide CreateLyricSlide(string lyrics)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),

                    // Lyrics text box
                    new P.Shape(
                        new P.NonVisualShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 2, Name = "Lyrics" },
                            new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                            new P.ApplicationNonVisualDrawingProperties()
                        ),
                        new P.ShapeProperties(
                            new D.Transform2D(
                                new D.Offset() { X = 914400L, Y = 1000000L },
                                new D.Extents() { Cx = 10297200L, Cy = 4858000L }
                            )
                        ),
                        CreateLyricTextBody(lyrics)
                    )
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );
    }

    private static P.TextBody CreateLyricTextBody(string lyrics)
    {
        var textBody = new P.TextBody(
            new D.BodyProperties()
            {
                Anchor = D.TextAnchoringTypeValues.Center,
                Wrap = D.TextWrappingValues.Square
            },
            new D.ListStyle()
        );

        var lines = lyrics.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            textBody.Append(new D.Paragraph(
                new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                new D.Run(
                    new D.RunProperties() { FontSize = 3200 },
                    new D.Text(line.Trim())
                )
            ));
        }

        // Se não há linhas, adiciona um parágrafo vazio
        if (lines.Length == 0)
        {
            textBody.Append(new D.Paragraph(new D.Run(new D.Text(""))));
        }

        return textBody;
    }

    private static D.Paragraph[] CreateLyricParagraphs(string lyrics)
    {
        var lines = lyrics.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var paragraphs = new List<D.Paragraph>();

        foreach (var line in lines)
        {
            paragraphs.Add(new D.Paragraph(
                new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                new D.Run(
                    new D.RunProperties() { FontSize = 3200 },
                    new D.Text(line.Trim())
                )
            ));
        }

        return paragraphs.Count > 0 ? paragraphs.ToArray() : new[] {
            new D.Paragraph(new D.Run(new D.Text("")))
        };
    }

    /// <summary>
    /// Splits lyrics into manageable sections for slides
    /// </summary>
    private List<string> SplitLyrics(string lyrics)
    {
        var sections = new List<string>();
        if (string.IsNullOrWhiteSpace(lyrics))
            return sections;

        // Split by double line breaks (verse separators) or every 6-8 lines
        var lines = lyrics.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var currentSection = new List<string>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Empty line indicates section break
                if (currentSection.Count > 0)
                {
                    sections.Add(string.Join("\n", currentSection));
                    currentSection.Clear();
                }
            }
            else
            {
                currentSection.Add(line.Trim());

                // If section gets too long, split it
                if (currentSection.Count >= 8)
                {
                    sections.Add(string.Join("\n", currentSection));
                    currentSection.Clear();
                }
            }
        }

        // Add remaining lines
        if (currentSection.Count > 0)
        {
            sections.Add(string.Join("\n", currentSection));
        }

        return sections.Count > 0 ? sections : new List<string> { lyrics };
    }

    /// <summary>
    /// Sanitizes filename to remove invalid characters
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 100 ? sanitized.Substring(0, 100) + ".pptx" : sanitized;
    }
}
