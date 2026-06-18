using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class SettingsSaveService : BackgroundService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly RcsState _rcsState;
    private readonly TbState _tbState;
    private readonly EnemyEspState _enemyEspState;
    private readonly SoundEspState _soundEspState;
    private readonly AimHelperState _aimHelperState;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly IOptionsMonitor<ToolkitOptions> _optionsMonitor;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SettingsSaveService> _logger;
    private Keys _saveKey = Keys.None;

    public SettingsSaveService(
        ToolkitEventBus eventBus,
        RcsState rcsState,
        TbState tbState,
        EnemyEspState enemyEspState,
        SoundEspState soundEspState,
        AimHelperState aimHelperState,
        ScreenOverlayManager overlayManager,
        IOptionsMonitor<ToolkitOptions> optionsMonitor,
        IHostEnvironment hostEnvironment,
        ILogger<SettingsSaveService> logger)
    {
        _eventBus = eventBus;
        _rcsState = rcsState;
        _tbState = tbState;
        _enemyEspState = enemyEspState;
        _soundEspState = soundEspState;
        _aimHelperState = aimHelperState;
        _overlayManager = overlayManager;
        _optionsMonitor = optionsMonitor;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _saveKey = KeyParser.Parse(_optionsMonitor.CurrentValue.SaveSettingsKey);
        if (_saveKey == Keys.None)
        {
            _logger.LogInformation("Settings save disabled (SaveSettingsKey is empty or invalid)");
            return Task.CompletedTask;
        }

        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation(
            "Settings save bound to {SaveKey} (writes full Toolkit section to appsettings.json)",
            _optionsMonitor.CurrentValue.SaveSettingsKey);

        stoppingToken.Register(() => _eventBus.OnKeyPress -= OnKeyPress);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress -= OnKeyPress;
        return base.StopAsync(cancellationToken);
    }

    private void OnKeyPress(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _saveKey)
            return;

        try
        {
            var toolkit = ToolkitOptionsCollector.Collect(
                _optionsMonitor.CurrentValue,
                _rcsState,
                _tbState,
                _enemyEspState,
                _soundEspState,
                _aimHelperState);

            var filePath = Path.Combine(_hostEnvironment.ContentRootPath, "appsettings.json");
            AppSettingsWriter.SaveToolkitSection(filePath, toolkit);

            _logger.LogInformation("Saved full Toolkit settings to {FilePath}", filePath);
            ShowSavedMessage(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to appsettings.json");
            ShowSaveFailedMessage();
        }
    }

    private void ShowSavedMessage(string filePath)
    {
        var layer = _overlayManager.GetOrCreateLayer("system", zIndex: 1000);
        _overlayManager.EnsureOnTop();

        var options = _optionsMonitor.CurrentValue;
        var color = DrawHelper.ParseColor(options.Overlay.InjectionPrompt.Color, Color.White);
        var fontSize = options.Overlay.InjectionPrompt.FontSize;

        layer.QueueDraw(g => DrawHelper.DrawTextTopRight(
            g,
            $"Settings saved to {Path.GetFileName(filePath)}",
            color,
            fontSize));
    }

    private void ShowSaveFailedMessage()
    {
        var layer = _overlayManager.GetOrCreateLayer("system", zIndex: 1000);
        _overlayManager.EnsureOnTop();

        layer.QueueDraw(g => DrawHelper.DrawTextTopRight(
            g,
            "Failed to save settings",
            Color.OrangeRed,
            _optionsMonitor.CurrentValue.Overlay.InjectionPrompt.FontSize));
    }
}
