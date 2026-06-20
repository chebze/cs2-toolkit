using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class BombStatusOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 100;
    private const uint DefaultColor = 0xFFFFD166;

    private readonly IActiveConfiguration _configuration;

    public BombStatusOverlayPresenter(IActiveConfiguration configuration) =>
        _configuration = configuration;

    public string LayerName => "bomb-status";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var panel = _configuration.Current.Profile.Visuals.BombStatus;
        if (!panel.Enabled || !snapshot.IsInMatch || !snapshot.Bomb.IsVisible)
            return [];

        var color = OverlayColorParser.ParseArgb(panel.Color, DefaultColor);
        var lines = BombStatusFormatter.BuildLines(snapshot.Bomb);
        return OverlayTextBuilder.BuildBlock(panel.X, panel.Y, lines, color, panel.FontSize, ZIndex);
    }
}
