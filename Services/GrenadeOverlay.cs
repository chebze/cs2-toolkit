using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class GrenadeOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly GrenadeTrajectoryTracker _tracker;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<GrenadeOverlay> _logger;
    private OverlayLayer? _layer;

    public GrenadeOverlay(
        ToolkitEventBus eventBus,
        GrenadeTrajectoryTracker tracker,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<GrenadeOverlay> logger)
    {
        _eventBus = eventBus;
        _tracker = tracker;
        _overlayManager = overlayManager;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("grenade-trajectory", zIndex: 110);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _layer.QueueDraw(DrawTrajectory);
        _logger.LogInformation("GrenadeOverlay subscribed to OnMemoryRead");
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
        if (_layer is null || !_options.Overlay.GrenadeTrajectory.Enabled || !e.State.IsInMatch)
            return;

        _overlayManager.EnsureOnTop();
    }

    private void DrawTrajectory(Graphics graphics)
    {
        var panel = _options.Overlay.GrenadeTrajectory;
        if (!panel.Enabled)
            return;

        var snapshot = _tracker.Snapshot;
        if (!snapshot.IsActive)
            return;

        var bounds = GameWindowHelper.GetTargetBounds();
        GrenadeArcDrawer.DrawTrajectory(
            graphics,
            snapshot,
            _tracker.LatestViewMatrix,
            bounds.Width,
            bounds.Height,
            panel,
            _options.Grenade.LandingMarkerRadiusUnits);
    }
}
