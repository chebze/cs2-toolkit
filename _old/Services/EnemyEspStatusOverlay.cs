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

public sealed class EnemyEspStatusOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly EnemyEspState _espState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<EnemyEspStatusOverlay> _logger;
    private OverlayLayer? _layer;

    public EnemyEspStatusOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        EnemyEspState espState,
        IOptions<ToolkitOptions> options,
        ILogger<EnemyEspStatusOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _espState = espState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("enemy-esp-status", zIndex: 110);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("EnemyEspStatusOverlay subscribed to OnMemoryRead");
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

        var panel = _options.Overlay.EspStatus;
        var mode = _espState.Mode;
        var (label, colorHex, fallback) = mode switch
        {
            EnemyEspMode.Full => ("ESP Full", panel.FullColor, Color.LimeGreen),
            EnemyEspMode.LastSeen => ("ESP Last", panel.LastSeenColor, Color.Goldenrod),
            _ => ("ESP", panel.DisabledColor, Color.Red)
        };

        var color = DrawHelper.ParseColor(colorHex, fallback);

        _layer.QueueDraw(g => DrawHelper.DrawTextBottomLeft(
            g, label, color, panel.FontSize, panel.Margin, lineIndexFromBottom: 2));
    }
}
