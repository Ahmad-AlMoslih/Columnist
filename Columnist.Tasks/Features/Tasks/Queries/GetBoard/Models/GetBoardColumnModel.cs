using System.Globalization;
using System.Text.RegularExpressions;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed partial record GetBoardColumnModel
{
    private const string Ink = "#1F2A44";

    public string ColumnToken { get; init; } = "";
    public string Name { get; init; } = "";
    public string StatusColor { get; init; } = "";

    // The column's colours from its status: the background is the status colour, the header band is it mixed 80/20 with the ink.
    // Null when the status has no valid #RRGGBB colour (the neutral look).
    public string? ColumnColor => HexColor().IsMatch(StatusColor ?? "") ? StatusColor!.ToUpperInvariant() : null;
    public string? HeaderColor => ColumnColor is null ? null : Mix(ColumnColor, Ink, 0.2);

    // The header text: the ink, or white if the band is too dark for the ink (under 4.5:1).
    public string? HeaderClass => HeaderColor is null
        ? null
        : Contrast(HeaderColor, Ink) >= 4.5 ? "kb-column-header-ink" : "kb-column-header-light";

    public IReadOnlyList<GetBoardTaskModel> Tasks { get; init; } = [];   // shown (after the search and the filter)
    public int TotalTaskCount { get; init; }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();

    private static (int R, int G, int B) Channels(string hex) => (
        int.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber),
        int.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber),
        int.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber));

    // weight = the share of "with".
    private static string Mix(string color, string with, double weight)
    {
        var a = Channels(color);
        var b = Channels(with);
        int Blend(int x, int y) => (int)Math.Round(x * (1 - weight) + y * weight);
        return $"#{Blend(a.R, b.R):X2}{Blend(a.G, b.G):X2}{Blend(a.B, b.B):X2}";
    }

    // WCAG 2 contrast ratio.
    private static double Contrast(string first, string second)
    {
        var l1 = Luminance(first);
        var l2 = Luminance(second);
        return (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
    }

    private static double Luminance(string hex)
    {
        var (r, g, b) = Channels(hex);
        static double Linear(int channel)
        {
            var c = channel / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Linear(r) + 0.7152 * Linear(g) + 0.0722 * Linear(b);
    }
}
