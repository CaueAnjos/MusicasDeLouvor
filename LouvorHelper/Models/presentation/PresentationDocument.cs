using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using LouvorHelper.Utils;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace LouvorHelper.Models.Presentation;

/// <summary>
/// Main presentation document class - handles creating PowerPoint presentations from music data
/// </summary>
internal class PresentationDocument
{
    private readonly Music _music;
    private readonly List<Slide> _slides;
    private string? _templatePath;

    public PresentationDocument(Music music)
    {
        _music = music;
        _slides = new List<Slide>();
        GenerateSlidesFromMusic();
    }

    /// <summary>
    /// Sets the PowerPoint template to use for the presentation
    /// </summary>
    /// <param name="templatePath">Path to the PowerPoint template file</param>
    /// <returns>This PresentationDocument instance for method chaining</returns>
    public PresentationDocument SetTemplate(string templatePath)
    {
        if (!TemplateManager.IsValidTemplate(templatePath))
        {
            throw new ArgumentException($"Invalid template file: {templatePath}");
        }

        _templatePath = templatePath;
        return this;
    }

    /// <summary>
    /// Adds a slide to the presentation
    /// </summary>
    /// <param name="slide">The slide to add</param>
    public void AddSlide(Slide slide)
    {
        _slides.Add(slide);
    }

    /// <summary>
    /// Inserts a slide at the specified position
    /// </summary>
    /// <param name="index">The position to insert the slide</param>
    /// <param name="slide">The slide to insert</param>
    public void InsertSlide(int index, Slide slide)
    {
        if (index < 0 || index > _slides.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _slides.Insert(index, slide);
    }

    /// <summary>
    /// Removes a slide at the specified position
    /// </summary>
    /// <param name="index">The position of the slide to remove</param>
    public void RemoveSlide(int index)
    {
        if (index >= 0 && index < _slides.Count)
        {
            _slides.RemoveAt(index);
        }
    }

    /// <summary>
    /// Gets the current slides in the presentation
    /// </summary>
    public IReadOnlyList<Slide> Slides => _slides.AsReadOnly();

    /// <summary>
    /// Saves the presentation to the specified file path
    /// </summary>
    /// <param name="filepath">The path where the presentation will be saved</param>
    public void Save(string filepath)
    {
        try
        {
            if (!string.IsNullOrEmpty(_templatePath))
            {
                CreatePresentationFromTemplate(filepath);
            }
            else
            {
                CreatePresentationFromScratch(filepath);
            }
        }
        catch (Exception ex)
        {
            Notify.Error($"Erro ao criar apresentação para {_music.Title}: {ex.Message}");
            throw;
        }
    }

    private void GenerateSlidesFromMusic()
    {
        // Add title slide
        _slides.Add(Slide.CreateTitleSlide(_music.Title, _music.Artist));

        // Add lyrics slides
        if (!string.IsNullOrEmpty(_music.Lyrics))
        {
            var lyricSections = LyricsSplitter.SplitLyrics(_music.Lyrics);
            foreach (var section in lyricSections)
            {
                _slides.Add(Slide.CreateLyricsSlide(section));
            }
        }
    }

    private void CreatePresentationFromTemplate(string filepath)
    {
        using var presentationDoc = TemplateManager.CreateFromTemplate(_templatePath!, filepath);
        var presentationPart = presentationDoc.PresentationPart!;

        // Clear existing slides but keep master
        ClearExistingSlides(presentationPart);

        // Add our slides
        AddSlidesToPresentation(presentationPart);

        presentationPart.Presentation.Save();
    }

    private void CreatePresentationFromScratch(string filepath)
    {
        using var presentationDoc = DocumentFormat.OpenXml.Packaging.PresentationDocument.Create(
            filepath,
            PresentationDocumentType.Presentation
        );

        var presentationPart = presentationDoc.AddPresentationPart();
        presentationPart.Presentation = new P.Presentation();

        // Create slide master and layouts
        var slideMasterPart = CreateSlideMaster(presentationPart);
        var layoutManager = new SlideLayoutManager(presentationPart);
        layoutManager.CreateDefaultLayouts(slideMasterPart);

        // Add slides
        AddSlidesToPresentation(presentationPart);

        // Set slide size (16:9 widescreen)
        presentationPart.Presentation.SlideSize = new P.SlideSize()
        {
            Cx = 12192000, // 16:9 width
            Cy = 6858000, // 16:9 height
            Type = P.SlideSizeValues.Screen16x9,
        };

        presentationPart.Presentation.Save();
    }

    private void AddSlidesToPresentation(PresentationPart presentationPart)
    {
        var slideIdList = new P.SlideIdList();
        uint slideId = 256;
        int relationshipId = GetNextRelationshipId(presentationPart);

        foreach (var slide in _slides)
        {
            var slidePart = presentationPart.AddNewPart<SlidePart>($"rId{relationshipId}");
            slidePart.Slide = SlideFactory.CreateSlide(slide);
            slidePart.Slide.Save();

            slideIdList.Append(
                new P.SlideId()
                {
                    Id = (UInt32Value)slideId++,
                    RelationshipId = $"rId{relationshipId++}",
                }
            );
        }

        presentationPart.Presentation.SlideIdList = slideIdList;
    }

    private static void ClearExistingSlides(PresentationPart presentationPart)
    {
        // Remove existing slides but keep the structure
        var slideIdList = presentationPart.Presentation.SlideIdList;
        if (slideIdList != null)
        {
            var slideIds = slideIdList.Elements<P.SlideId>().ToList();
            foreach (var slideId in slideIds)
            {
                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                presentationPart.DeletePart(slidePart);
                slideId.Remove();
            }
        }
    }

    private static int GetNextRelationshipId(PresentationPart presentationPart)
    {
        // Find the highest relationship ID and return next available
        int maxId = 1;
        foreach (var part in presentationPart.Parts)
        {
            if (part.RelationshipId.StartsWith("rId"))
            {
                if (int.TryParse(part.RelationshipId.Substring(3), out int id))
                {
                    maxId = Math.Max(maxId, id);
                }
            }
        }
        return maxId + 1;
    }

    private SlideMasterPart CreateSlideMaster(PresentationPart presentationPart)
    {
        var slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>("rId1");
        slideMasterPart.SlideMaster = new P.SlideMaster(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup())
                )
            ),
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
                FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink,
            }
        );

        presentationPart.Presentation.SlideMasterIdList = new P.SlideMasterIdList(
            new P.SlideMasterId() { Id = (UInt32Value)2147483648U, RelationshipId = "rId1" }
        );

        return slideMasterPart;
    }
}
