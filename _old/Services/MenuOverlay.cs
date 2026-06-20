using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class MenuOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly IOptionsMonitor<ToolkitOptions> _optionsMonitor;
    private readonly ILogger<MenuOverlay> _logger;

    private OverlayLayer? _layer;
    private bool _isVisible;
    private Keys _menuToggleKey;

    public MenuOverlay(
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        IOptionsMonitor<ToolkitOptions> optionsMonitor,
        ILogger<MenuOverlay> logger)
    {
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _menuToggleKey = KeyParser.Parse(_optionsMonitor.CurrentValue.MenuToggleKey);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("menu", zIndex: 500);
        _eventBus.OnKeyPress += OnKeyPress;
        _eventBus.OnMouseMove += OnMouseMove;
        _optionsMonitor.OnChange(_ => _menuToggleKey = KeyParser.Parse(_.MenuToggleKey));
        _logger.LogInformation("MenuOverlay started — toggle with {Key}", _optionsMonitor.CurrentValue.MenuToggleKey);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress -= OnKeyPress;
        _eventBus.OnMouseMove -= OnMouseMove;
        return Task.CompletedTask;
    }

    private void OnKeyPress(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _menuToggleKey)
            return;

        _isVisible = !_isVisible;
        _overlayManager.SetInteractive(_isVisible);
        QueueMenu();
    }

    private void OnMouseMove(object? sender, MouseInputEventArgs e)
    {
        if (!_isVisible)
            return;

        QueueMenu();
    }

    private void QueueMenu()
    {
        if (_layer is null)
            return;

        if (!_isVisible)
        {
            _layer.ClearQueue();
            return;
        }

        var options = _optionsMonitor.CurrentValue;
        var menu = options.Overlay.Menu;
        var enemy = options.Overlay.EnemyLastSeen;
        var teammate = options.Overlay.TeammateStats;

        var bgColor = DrawHelper.ParseColor(menu.BackgroundColor, Color.FromArgb(200, 30, 30, 46));
        var textColor = DrawHelper.ParseColor(menu.TextColor, Color.White);

        var lines = new[]
        {
            "CS2 Toolkit Settings",
            "",
            $"Inject Key: {options.InjectKey}",
            $"Menu Toggle: {options.MenuToggleKey}",
            $"Panic Key: {options.PanicKey}",
            $"Memory Interval: {options.MemoryReadIntervalMs}ms",
            "",
            "Enemy Last Seen",
            $"  Color: {enemy.Color}",
            $"  Line Width: {enemy.LineWidth}",
            "",
            "Teammate Stats",
            $"  Position: ({teammate.X}, {teammate.Y})",
            $"  Color: {teammate.Color}",
            $"  Font Size: {teammate.FontSize}",
            "",
            $"Press {options.MenuToggleKey} to close"
        };

        _layer.QueueDraw(g =>
        {
            var padding = 12;
            using var font = new Font("Segoe UI", menu.FontSize, FontStyle.Regular, GraphicsUnit.Point);
            var maxWidth = lines.Max(line => g.MeasureString(line, font).Width);
            var lineHeight = menu.FontSize + 6;
            var boxWidth = (int)maxWidth + padding * 2;
            var boxHeight = lines.Length * lineHeight + padding * 2;

            using var background = new SolidBrush(bgColor);
            g.FillRectangle(background, menu.X, menu.Y, boxWidth, boxHeight);

            DrawHelper.DrawTextBlock(g, menu.X + padding, menu.Y + padding, lines, textColor, menu.FontSize);
        });
    }
}
