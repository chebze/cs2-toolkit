using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class EnemyOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly EnemyLastSeenTracker _lastSeenTracker;
    private readonly EnemyEspState _espState;
    private readonly ViewMatrixHolder _viewMatrixHolder;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<EnemyOverlay> _logger;
    private OverlayLayer? _layer;

    public EnemyOverlay(
        ToolkitEventBus eventBus,
        EnemyLastSeenTracker lastSeenTracker,
        EnemyEspState espState,
        ViewMatrixHolder viewMatrixHolder,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<EnemyOverlay> logger)
    {
        _eventBus = eventBus;
        _lastSeenTracker = lastSeenTracker;
        _espState = espState;
        _viewMatrixHolder = viewMatrixHolder;
        _overlayManager = overlayManager;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("enemy-last-seen", zIndex: 100);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _layer.QueueDraw(DrawSkeletons);
        _logger.LogInformation("EnemyOverlay subscribed to OnMemoryRead");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnMemoryRead -= OnMemoryRead;
        _layer?.ClearQueue();
        return Task.CompletedTask;
    }

    private void OnMemoryRead(object? sender, MemoryReadEventArgs e)
    {
        if (_layer is null || _espState.Mode == EnemyEspMode.Disabled || !e.State.IsInMatch)
            return;

        _overlayManager.EnsureOnTop();
    }

    private void DrawSkeletons(Graphics graphics)
    {
        var mode = _espState.Mode;
        if (mode == EnemyEspMode.Disabled)
            return;

        var panel = _options.Overlay.EnemyLastSeen;
        var bounds = GameWindowHelper.GetTargetBounds();
        var color = DrawHelper.ParseColor(panel.Color, Color.OrangeRed);
        var snapshots = mode switch
        {
            EnemyEspMode.Full => _lastSeenTracker.CopyLiveSnapshots(),
            EnemyEspMode.LastSeen => _lastSeenTracker.CopyDrawableSnapshots(),
            _ => []
        };

        if (snapshots.Count == 0)
            return;

        Span<float> viewMatrix = stackalloc float[16];
        _viewMatrixHolder.CopyTo(viewMatrix);

        SkeletonDrawer.DrawLastSeen(
            graphics,
            snapshots,
            viewMatrix,
            bounds.Width,
            bounds.Height,
            color,
            panel.LineWidth);
    }
}
