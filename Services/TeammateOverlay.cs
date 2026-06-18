using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class TeammateOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<TeammateOverlay> _logger;
    private OverlayLayer? _layer;

    public TeammateOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<TeammateOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Overlay.TeammateStats.Enabled)
        {
            _logger.LogInformation("TeammateOverlay disabled");
            return Task.CompletedTask;
        }

        _layer = _overlayManager.GetOrCreateLayer("teammate-stats", zIndex: 100);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("TeammateOverlay subscribed to OnMemoryRead");
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

        if (!e.State.IsInMatch)
        {
            _layer.ClearQueue();
            return;
        }

        _overlayManager.EnsureOnTop();

        var panel = _options.Overlay.TeammateStats;
        var color = DrawHelper.ParseColor(panel.Color, Color.MediumSeaGreen);

        var lines = new[]
        {
            "Teammates",
            $"  Alive: {e.State.TeammatesAlive}",
            $"  Dead:  {e.State.TeammatesDead}"
        };

        _layer.QueueDraw(g => DrawHelper.DrawTextBlock(g, panel.X, panel.Y, lines, color, panel.FontSize));
    }
}
