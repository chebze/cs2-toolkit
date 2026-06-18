using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class RcsOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly RcsState _rcsState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<RcsOverlay> _logger;
    private OverlayLayer? _layer;

    public RcsOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        RcsState rcsState,
        IOptions<ToolkitOptions> options,
        ILogger<RcsOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _rcsState = rcsState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("rcs-status", zIndex: 110);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("RcsOverlay subscribed to OnMemoryRead");
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

        var panel = _options.Overlay.RcsStatus;
        var enabled = _rcsState.IsEnabled;
        var color = DrawHelper.ParseColor(
            enabled ? panel.EnabledColor : panel.DisabledColor,
            enabled ? Color.LimeGreen : Color.Red);

        _layer.QueueDraw(g => DrawHelper.DrawTextBottomLeft(
            g, "RCS", color, panel.FontSize, panel.Margin, lineIndexFromBottom: 0));
    }
}
