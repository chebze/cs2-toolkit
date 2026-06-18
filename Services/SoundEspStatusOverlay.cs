using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class SoundEspStatusOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly SoundEspState _soundEspState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<SoundEspStatusOverlay> _logger;
    private OverlayLayer? _layer;

    public SoundEspStatusOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        SoundEspState soundEspState,
        IOptions<ToolkitOptions> options,
        ILogger<SoundEspStatusOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _soundEspState = soundEspState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("sound-esp-status", zIndex: 110);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _logger.LogInformation("SoundEspStatusOverlay subscribed to OnMemoryRead");
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

        var panel = _options.Overlay.SoundEspStatus;
        var enabled = _soundEspState.IsEnabled;
        var color = DrawHelper.ParseColor(
            enabled ? panel.EnabledColor : panel.DisabledColor,
            enabled ? Color.LimeGreen : Color.Red);

        _layer.QueueDraw(g => DrawHelper.DrawTextBottomLeft(
            g, "S-ESP", color, panel.FontSize, panel.Margin, lineIndexFromBottom: 3));
    }
}
