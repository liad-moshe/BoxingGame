using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BoxingGame.Components;

// Renders an SVG <text> element via BuildRenderTree to avoid Blazor's
// Razor parser treating <text> as its own special directive (RZ1023).
public class SvgText : ComponentBase
{
    [Parameter] public string? X            { get; set; }
    [Parameter] public string? Y            { get; set; }
    [Parameter] public string? FontSize     { get; set; }
    [Parameter] public string? Fill         { get; set; }
    [Parameter] public string? FontFamily   { get; set; }
    [Parameter] public string? FontWeight   { get; set; }
    [Parameter] public string? TextAnchor   { get; set; }
    [Parameter] public string? Stroke       { get; set; }
    [Parameter] public string? StrokeWidth  { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "text");
        if (X           != null) builder.AddAttribute(1,  "x",            X);
        if (Y           != null) builder.AddAttribute(2,  "y",            Y);
        if (FontSize    != null) builder.AddAttribute(3,  "font-size",    FontSize);
        if (Fill        != null) builder.AddAttribute(4,  "fill",         Fill);
        if (FontFamily  != null) builder.AddAttribute(5,  "font-family",  FontFamily);
        if (FontWeight  != null) builder.AddAttribute(6,  "font-weight",  FontWeight);
        if (TextAnchor  != null) builder.AddAttribute(7,  "text-anchor",  TextAnchor);
        if (Stroke      != null) builder.AddAttribute(8,  "stroke",       Stroke);
        if (StrokeWidth != null) builder.AddAttribute(9,  "stroke-width", StrokeWidth);
        if (AdditionalAttributes != null)
            builder.AddMultipleAttributes(10, AdditionalAttributes);
        builder.AddContent(11, ChildContent);
        builder.CloseElement();
    }
}
