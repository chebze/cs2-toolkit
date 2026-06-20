using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class MenuOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 500;
    private const uint DefaultBackgroundColor = 0xCC1E1E2E;
    private const uint DefaultTextColor = 0xFFFFFFFF;

    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _state;

    public MenuOverlayPresenter(IActiveConfiguration configuration, IFeatureState state)
    {
        _configuration = configuration;
        _state = state;
    }

    public string LayerName => "menu";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        if (!_state.IsEnabled(FeatureIds.Menu))
            return [];

        var settings = _configuration.Current;
        var menu = settings.Profile.Visuals.Menu;
        var enemy = settings.Profile.EnemyEsp;
        var teammate = settings.Profile.Visuals.TeammateStats;
        var keybinds = settings.Keybinds;

        var lines = new[]
        {
            "CS2 Toolkit Settings",
            "",
            $"Inject Key: {keybinds.InjectKey}",
            $"Menu Toggle: {keybinds.MenuToggleKey}",
            $"Panic Key: {keybinds.PanicKey}",
            $"Memory Interval: {settings.Host.MemoryReadIntervalMs}ms",
            "",
            "Enemy Last Seen",
            $"  Color: {enemy.SkeletonColor}",
            $"  Line Width: {enemy.SkeletonLineWidth}",
            "",
            "Teammate Stats",
            $"  Position: ({teammate.X}, {teammate.Y})",
            $"  Color: {teammate.Color}",
            $"  Font Size: {teammate.FontSize}",
            "",
            $"Press {keybinds.MenuToggleKey} to close"
        };

        var backgroundColor = OverlayColorParser.ParseArgb(menu.BackgroundColor, DefaultBackgroundColor);
        var textColor = OverlayColorParser.ParseArgb(menu.TextColor, DefaultTextColor);

        return MenuPanelDrawBuilder.Build(
            menu.X,
            menu.Y,
            lines,
            backgroundColor,
            textColor,
            menu.FontSize,
            ZIndex);
    }
}
