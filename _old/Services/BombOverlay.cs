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

public sealed class BombOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<BombOverlay> _logger;
    private OverlayLayer? _layer;

    public BombOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<BombOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("bomb-status", zIndex: 100);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("BombOverlay subscribed to OnMemoryRead");
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

        if (!e.State.IsInMatch || !e.State.Bomb.IsVisible)
        {
            _layer.ClearQueue();
            return;
        }

        _overlayManager.EnsureOnTop();

        var panel = _options.Overlay.BombStatus;
        var color = DrawHelper.ParseColor(panel.Color, Color.Goldenrod);
        var lines = BuildLines(e.State.Bomb);

        _layer.QueueDraw(g => DrawHelper.DrawTextBlock(g, panel.X, panel.Y, lines, color, panel.FontSize));
    }

    private static string[] BuildLines(BombInfo bomb)
    {
        var lines = new List<string> { "Bomb", $"  {FormatStatus(bomb.Status)}" };

        if (bomb.Status is BombStatus.Planting or BombStatus.Planted && !string.IsNullOrEmpty(bomb.Site))
            lines.Add($"  Site: {bomb.Site}");

        if (bomb.Status == BombStatus.Planted && bomb.TimeLeftSeconds is not null)
            lines.Add($"  Time left: {bomb.TimeLeftSeconds}s");

        if (bomb.Status == BombStatus.Defusing)
        {
            if (bomb.TimeLeftSeconds is not null)
                lines.Add($"  Time left: {bomb.TimeLeftSeconds}s");

            lines.Add($"  Kit: {FormatYesNo(bomb.HasDefuseKit)}");

            if (bomb.DefuseTimeSeconds is not null)
                lines.Add($"  Time to defuse: {bomb.DefuseTimeSeconds}s");

            lines.Add($"  Will succeed: {FormatYesNo(bomb.WillDefuseSucceed)}");
        }

        return lines.ToArray();
    }

    private static string FormatStatus(BombStatus status) => status switch
    {
        BombStatus.Carried => "Carried",
        BombStatus.Equipped => "Equipped",
        BombStatus.OnGround => "On ground",
        BombStatus.Defusing => "Defusing",
        BombStatus.Planting => "Planting",
        BombStatus.Planted => "Planted",
        _ => string.Empty
    };

    private static string FormatYesNo(bool? value) => value switch
    {
        true => "yes",
        false => "no",
        _ => "unknown"
    };
}
