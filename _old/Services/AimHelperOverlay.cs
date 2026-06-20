using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Models;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class AimHelperOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly AimHelperState _aimHelperState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<AimHelperOverlay> _logger;
    private OverlayLayer? _layer;

    public AimHelperOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        AimHelperState aimHelperState,
        IOptions<ToolkitOptions> options,
        ILogger<AimHelperOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _aimHelperState = aimHelperState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("aim-helper-status", zIndex: 110);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("AimHelperOverlay subscribed to OnMemoryRead");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnMemoryRead -= OnMemoryRead;
        return Task.CompletedTask;
    }

    private void OnMemoryRead(object? sender, MemoryReadEventArgs e)
    {
        if (_layer is null)
            return;

        if (!e.State.IsAttached)
        {
            _layer.ClearQueue();
            return;
        }

        _overlayManager.EnsureOnTop();

        var panel = _options.Overlay.AimHelperStatus;
        var aimOptions = _options.AimHelper;
        var enabled = _aimHelperState.IsEnabled;
        var color = DrawHelper.ParseColor(
            enabled ? panel.EnabledColor : panel.DisabledColor,
            enabled ? Color.LimeGreen : Color.Red);
        var fovColor = DrawHelper.ParseColor(aimOptions.FovCircleColor, Color.Cyan);
        var inMatch = e.State.IsInMatch;
        var boneLabel = AimHelperBoneParser.ToLabel(_aimHelperState.PreferredBone);

        _layer.QueueDraw(g =>
        {
            if (enabled && inMatch)
            {
                var layout = DrawHelper.GetAngularFovCircleLayout(
                    g,
                    _aimHelperState.FovDegrees,
                    aimOptions.AssumedHorizontalFovDegrees);

                DrawHelper.DrawAngularFovCircle(
                    g,
                    _aimHelperState.FovDegrees,
                    fovColor,
                    aimOptions.FovCircleLineWidth,
                    aimOptions.AssumedHorizontalFovDegrees);

                if (layout.IsValid)
                {
                    DrawHelper.DrawTextRightOfPoint(
                        g,
                        layout.CenterX + layout.RadiusPixels,
                        layout.CenterY,
                        [$"{_aimHelperState.FovDegrees:F1}°", boneLabel],
                        fovColor,
                        Math.Max(1, panel.FontSize / 2));
                }
            }

            DrawHelper.DrawTextBottomLeft(
                g, "AIM", color, panel.FontSize, panel.Margin, lineIndexFromBottom: 4);
        });
    }
}
