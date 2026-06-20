using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class TeammateStatsOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 100;
    private const uint DefaultColor = 0xFF6BCB77;

    private readonly IActiveConfiguration _configuration;

    public TeammateStatsOverlayPresenter(IActiveConfiguration configuration) =>
        _configuration = configuration;

    public string LayerName => "teammate-stats";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var panel = _configuration.Current.Profile.Visuals.TeammateStats;
        if (!panel.Enabled || !snapshot.IsInMatch)
            return [];

        var color = OverlayColorParser.ParseArgb(panel.Color, DefaultColor);
        var lines = new[]
        {
            "Teammates",
            $"  Alive: {snapshot.TeammatesAlive}",
            $"  Dead:  {snapshot.TeammatesDead}"
        };

        return OverlayTextBuilder.BuildBlock(panel.X, panel.Y, lines, color, panel.FontSize, ZIndex);
    }
}
