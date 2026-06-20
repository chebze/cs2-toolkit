using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class ClairvoyanceOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 100;
    private const uint DefaultColor = 0xFFB794F4;

    private readonly IActiveConfiguration _configuration;

    public ClairvoyanceOverlayPresenter(IActiveConfiguration configuration) =>
        _configuration = configuration;

    public string LayerName => "clairvoyance";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var panel = _configuration.Current.Profile.Visuals.Clairvoyance;
        if (!panel.Enabled || !snapshot.IsInMatch)
            return [];

        var color = OverlayColorParser.ParseArgb(panel.Color, DefaultColor);
        var lines = BuildLines(snapshot.ClairvoyanceTips);
        return OverlayTextBuilder.BuildBlock(panel.X, panel.Y, lines, color, panel.FontSize, ZIndex);
    }

    private static IReadOnlyList<string> BuildLines(IReadOnlyList<string> tips)
    {
        var lines = new List<string> { "Clairvoyance" };
        foreach (var tip in tips)
            lines.Add($"  {tip}");

        return lines;
    }
}
