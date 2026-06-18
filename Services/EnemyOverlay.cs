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
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<EnemyOverlay> _logger;
    private OverlayLayer? _layer;
    private DateTime _lastDrawLogAt;

    public EnemyOverlay(
        ToolkitEventBus eventBus,
        EnemyLastSeenTracker lastSeenTracker,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<EnemyOverlay> logger)
    {
        _eventBus = eventBus;
        _lastSeenTracker = lastSeenTracker;
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
        if (_layer is null || !e.State.IsInMatch)
            return;

        _overlayManager.EnsureOnTop();
    }

    private void DrawSkeletons(Graphics graphics)
    {
        var panel = _options.Overlay.EnemyLastSeen;
        var bounds = GameWindowHelper.GetTargetBounds();
        var color = DrawHelper.ParseColor(panel.Color, Color.OrangeRed);
        var snapshots = _lastSeenTracker.DrawableSnapshots.ToList();

        SkeletonDrawer.DrawLastSeen(
            graphics,
            snapshots,
            _lastSeenTracker.LatestViewMatrix,
            bounds.Width,
            bounds.Height,
            color,
            panel.LineWidth);

        LogSkeletonDraw(snapshots, panel, bounds.Width, bounds.Height);
    }

    private void LogSkeletonDraw(
        IReadOnlyList<EnemyLastSeenSnapshot> snapshots,
        SkeletonOverlayOptions panel,
        int screenWidth,
        int screenHeight)
    {
        if (!panel.LogDiagnostics || snapshots.Count == 0)
            return;

        var now = DateTime.UtcNow;
        if ((now - _lastDrawLogAt).TotalMilliseconds < panel.LogDiagnosticsIntervalMs)
            return;

        _lastDrawLogAt = now;
        var snapshot = snapshots[0];
        _logger.LogInformation(
            "{Diagnostics}",
            SkeletonDiagnostics.FormatDraw(snapshot, _lastSeenTracker.LatestViewMatrix, screenWidth, screenHeight));
    }
}
