using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

public sealed class TriggerbotOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 120;
    private const float DefaultPreFireFovDegrees = 0.7f;

    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _state;
    private readonly TriggerbotHostSettings _host;

    public TriggerbotOverlayPresenter(
        IActiveConfiguration configuration,
        IFeatureState state,
        IOptions<ToolkitHostSettings> hostSettings)
    {
        _configuration = configuration;
        _state = state;
        _host = hostSettings.Value.Triggerbot;
    }

    public string LayerName => "triggerbot-status";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var commands = new List<DrawCommand>();
        var enabled = _state.IsEnabled(FeatureIds.Triggerbot);
        var statusColor = enabled ? 0xFF22C55Eu : 0xFFEF4444u;

        commands.Add(new TextDrawCommand(
            _host.StatusMargin,
            screenHeight - _host.StatusMargin - _host.StatusFontSize - 6f,
            "TB",
            statusColor,
            _host.StatusFontSize,
            ZIndex: 900));

        if (!enabled || !snapshot.IsInMatch)
            return commands;

        var weaponId = (ushort)(snapshot.LocalPlayer?.ActiveWeaponId.Value ?? 0);
        var triggerbot = _configuration.ResolveWeapon(weaponId).Triggerbot;
        var preFireFov = triggerbot.PreFireFovDegrees ?? DefaultPreFireFovDegrees;
        var minDelay = triggerbot.MinReactionDelayMs ?? 200;
        var maxDelay = triggerbot.MaxReactionDelayMs ?? 400;
        var fovColor = OverlayColorParser.ParseArgb(_host.FovCircleColor, 0xFFEF4444);

        commands.AddRange(FovCircleDrawBuilder.BuildCenterCircle(
            preFireFov,
            _host.AssumedHorizontalFovDegrees,
            fovColor,
            _host.FovCircleLineWidth,
            screenWidth,
            screenHeight,
            ZIndex));

        if (FovCircleDrawBuilder.TryGetLayout(preFireFov, _host.AssumedHorizontalFovDegrees, screenWidth, screenHeight, out var layout))
        {
            var labelSize = Math.Max(1f, _host.StatusFontSize / 2f);
            commands.Add(new TextDrawCommand(
                layout.CenterX - layout.RadiusPixels - 4f,
                layout.CenterY,
                $"AS: {(_state.TriggerbotAutoStopEnabled ? "ON" : "OFF")}",
                fovColor,
                labelSize,
                ZIndex: ZIndex));

            commands.Add(new TextDrawCommand(
                layout.CenterX + layout.RadiusPixels + 4f,
                layout.CenterY - labelSize,
                $"{minDelay} ms",
                fovColor,
                labelSize,
                ZIndex: ZIndex));

            commands.Add(new TextDrawCommand(
                layout.CenterX + layout.RadiusPixels + 4f,
                layout.CenterY + 2f,
                $"{maxDelay} ms",
                fovColor,
                labelSize,
                ZIndex: ZIndex));
        }

        return commands;
    }
}
