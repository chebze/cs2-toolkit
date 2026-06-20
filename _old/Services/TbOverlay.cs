using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class TbOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly TbState _tbState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<TbOverlay> _logger;
    private OverlayLayer? _layer;

    public TbOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        TbState tbState,
        IOptions<ToolkitOptions> options,
        ILogger<TbOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _tbState = tbState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("tb-status", zIndex: 110);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("TbOverlay subscribed to OnMemoryRead");
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

        var panel = _options.Overlay.TbStatus;
        var tbOptions = _options.Tb;
        var enabled = _tbState.IsEnabled;
        var color = DrawHelper.ParseColor(
            enabled ? panel.EnabledColor : panel.DisabledColor,
            enabled ? Color.LimeGreen : Color.Red);
        var fovColor = DrawHelper.ParseColor(tbOptions.FovCircleColor, Color.Red);
        var inMatch = e.State.IsInMatch;

        _layer.QueueDraw(g =>
        {
            if (enabled && inMatch)
            {
                var layout = DrawHelper.GetAngularFovCircleLayout(
                    g,
                    _tbState.PreFireFovDegrees,
                    tbOptions.AssumedHorizontalFovDegrees);

                DrawHelper.DrawAngularFovCircle(
                    g,
                    _tbState.PreFireFovDegrees,
                    fovColor,
                    tbOptions.FovCircleLineWidth,
                    tbOptions.AssumedHorizontalFovDegrees);

                if (layout.IsValid)
                {
                    var labelFontSize = Math.Max(1, panel.FontSize / 2);
                    DrawHelper.DrawTextLeftOfPoint(
                        g,
                        layout.CenterX - layout.RadiusPixels,
                        layout.CenterY,
                        [$"AS: {(_tbState.IsAutoStopEnabled ? "ON" : "OFF")}"],
                        fovColor,
                        labelFontSize);

                    DrawHelper.DrawTextRightOfPoint(
                        g,
                        layout.CenterX + layout.RadiusPixels,
                        layout.CenterY,
                        [
                            $"{_tbState.MinReactionDelayMs} ms",
                            $"{_tbState.MaxReactionDelayMs} ms"
                        ],
                        fovColor,
                        Math.Max(1, panel.FontSize / 2));
                }
            }

            DrawHelper.DrawTextBottomLeft(
                g, "TB", color, panel.FontSize, panel.Margin, lineIndexFromBottom: 1);
        });
    }
}
