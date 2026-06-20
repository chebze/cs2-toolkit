using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

public sealed class AimHelperOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 110;
    private const float DefaultFovDegrees = 3f;

    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _state;
    private readonly AimHelperHostSettings _host;

    public AimHelperOverlayPresenter(
        IActiveConfiguration configuration,
        IFeatureState state,
        IOptions<ToolkitHostSettings> hostSettings)
    {
        _configuration = configuration;
        _state = state;
        _host = hostSettings.Value.AimHelper;
    }

    public string LayerName => "aim-helper-status";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var commands = new List<DrawCommand>();
        var enabled = _state.IsEnabled(FeatureIds.AimHelper);
        var statusColor = OverlayColorParser.ParseArgb(
            enabled ? _host.EnabledColor : _host.DisabledColor,
            enabled ? 0xFF22C55Eu : 0xFFEF4444u);

        var lineHeight = _host.StatusFontSize + 6f;

        commands.Add(new TextDrawCommand(
            _host.StatusMargin,
            screenHeight - _host.StatusMargin - lineHeight * 3f,
            "AIM",
            statusColor,
            _host.StatusFontSize,
            ZIndex: 900));

        if (!enabled || !snapshot.IsInMatch)
            return commands;

        var weaponId = (ushort)(snapshot.LocalPlayer?.ActiveWeaponId.Value ?? 0);
        var aimHelper = _configuration.ResolveWeapon(weaponId).AimHelper;
        var fovDegrees = aimHelper.FovDegrees ?? DefaultFovDegrees;
        var boneLabel = PreferredBoneParser.ToLabel(PreferredBoneParser.Parse(aimHelper.PreferredBone));
        var fovColor = OverlayColorParser.ParseArgb(_host.FovCircleColor, 0xFF38BDF8);

        commands.AddRange(FovCircleDrawBuilder.BuildCenterCircle(
            fovDegrees,
            _host.AssumedHorizontalFovDegrees,
            fovColor,
            _host.FovCircleLineWidth,
            screenWidth,
            screenHeight,
            ZIndex));

        if (FovCircleDrawBuilder.TryGetLayout(
                fovDegrees,
                _host.AssumedHorizontalFovDegrees,
                screenWidth,
                screenHeight,
                out var layout))
        {
            var labelSize = Math.Max(1f, _host.StatusFontSize / 2f);
            commands.Add(new TextDrawCommand(
                layout.CenterX + layout.RadiusPixels + 4f,
                layout.CenterY - labelSize,
                $"{fovDegrees:F1}°",
                fovColor,
                labelSize,
                ZIndex: ZIndex));

            commands.Add(new TextDrawCommand(
                layout.CenterX + layout.RadiusPixels + 4f,
                layout.CenterY + 2f,
                boneLabel,
                fovColor,
                labelSize,
                ZIndex: ZIndex));
        }

        return commands;
    }
}
