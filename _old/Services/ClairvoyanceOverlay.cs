using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class ClairvoyanceOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<ClairvoyanceOverlay> _logger;
    private OverlayLayer? _layer;

    public ClairvoyanceOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<ClairvoyanceOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("clairvoyance", zIndex: 100);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("ClairvoyanceOverlay subscribed to OnMemoryRead");
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

        var panel = _options.Overlay.Clairvoyance;
        var color = DrawHelper.ParseColor(panel.Color, Color.MediumPurple);
        var lines = BuildLines(e.State.ClairvoyanceTips);

        _layer.QueueDraw(g => DrawHelper.DrawTextBlock(g, panel.X, panel.Y, lines, color, panel.FontSize));
    }

    private static string[] BuildLines(IReadOnlyList<string> tips)
    {
        var lines = new List<string> { "Clairvoyance" };
        lines.AddRange(tips.Select(tip => $"  {tip}"));
        return lines.ToArray();
    }
}
