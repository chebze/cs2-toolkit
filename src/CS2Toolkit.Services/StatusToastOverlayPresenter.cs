using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class StatusToastOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 1000;
    private const uint DefaultColor = 0xFFFFFFFF;

    private readonly IActiveConfiguration _configuration;
    private readonly IStatusToastPublisher _toasts;

    public StatusToastOverlayPresenter(
        IActiveConfiguration configuration,
        IStatusToastPublisher toasts)
    {
        _configuration = configuration;
        _toasts = toasts;
    }

    public string LayerName => "system";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var active = _toasts.GetActive();
        if (active.Count == 0)
            return [];

        var panel = _configuration.Current.Profile.Visuals.SystemMessages;
        var color = OverlayColorParser.ParseArgb(panel.Color, DefaultColor);

        return TopRightTextDrawBuilder.Build(
            screenWidth,
            panel.Margin,
            active,
            color,
            panel.FontSize,
            ZIndex);
    }
}
