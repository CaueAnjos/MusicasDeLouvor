using DocumentFormat.OpenXml.Packaging;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace LouvorHelper.Models.Presentation;

/// <summary>
/// Manages slide layouts and their creation
/// </summary>
public class SlideLayoutManager
{
    private readonly PresentationPart _presentationPart;
    private readonly Dictionary<SlideLayoutType, SlideLayoutPart> _layouts;

    public SlideLayoutManager(PresentationPart presentationPart)
    {
        _presentationPart = presentationPart;
        _layouts = new Dictionary<SlideLayoutType, SlideLayoutPart>();
    }

    /// <summary>
    /// Creates default layouts for the presentation
    /// </summary>
    public void CreateDefaultLayouts(SlideMasterPart slideMasterPart)
    {
        // Create title layout
        var titleLayout = slideMasterPart.AddNewPart<SlideLayoutPart>("rId1");
        titleLayout.SlideLayout = CreateTitleLayout();
        _layouts[SlideLayoutType.Title] = titleLayout;

        // Create lyrics layout
        var lyricsLayout = slideMasterPart.AddNewPart<SlideLayoutPart>("rId2");
        lyricsLayout.SlideLayout = CreateLyricsLayout();
        _layouts[SlideLayoutType.Lyrics] = lyricsLayout;
    }

    /// <summary>
    /// Gets the layout part for the specified layout type
    /// </summary>
    public SlideLayoutPart GetLayout(SlideLayoutType layoutType)
    {
        return _layouts.TryGetValue(layoutType, out var layout)
            ? layout
            : _layouts[SlideLayoutType.Title];
    }

    private P.SlideLayout CreateTitleLayout()
    {
        return new P.SlideLayout(
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
            new P.ColorMapOverride(new D.MasterColorMapping())
        )
        {
            Type = P.SlideLayoutValues.Title,
        };
    }

    private P.SlideLayout CreateLyricsLayout()
    {
        return new P.SlideLayout(
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
            new P.ColorMapOverride(new D.MasterColorMapping())
        )
        {
            Type = P.SlideLayoutValues.Blank,
        };
    }
}
