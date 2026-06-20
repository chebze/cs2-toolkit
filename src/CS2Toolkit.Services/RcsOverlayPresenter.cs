using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

public sealed class RcsOverlayPresenter : IOverlayPresenter
{
    private readonly IFeatureState _state;
    private readonly RcsHostSettings _host;

    public RcsOverlayPresenter(IFeatureState state, IOptions<ToolkitHostSettings> hostSettings)
    {
        _state = state;
        _host = hostSettings.Value.Rcs;
    }

    public string LayerName => "rcs-status";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        if (!snapshot.IsAttached)
            return [];

        var enabled = _state.IsEnabled(FeatureIds.Rcs);
        var color = OverlayColorParser.ParseArgb(
            enabled ? _host.EnabledColor : _host.DisabledColor,
            enabled ? 0xFF22C55Eu : 0xFFEF4444u);

        var lineHeight = _host.StatusFontSize + 6f;
        return
        [
            new TextDrawCommand(
                _host.StatusMargin,
                screenHeight - _host.StatusMargin - lineHeight,
                "RCS",
                color,
                _host.StatusFontSize,
                ZIndex: 900)
        ];
    }
}
