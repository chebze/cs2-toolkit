using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class ConfigProfileSwitchService : BackgroundService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly ConfigManager _configManager;
    private readonly ILogger<ConfigProfileSwitchService> _logger;
    private readonly Dictionary<Keys, string> _hotkeyToProfileId = new();
    private readonly object _lock = new();

    public ConfigProfileSwitchService(
        ToolkitEventBus eventBus,
        ConfigManager configManager,
        ILogger<ConfigProfileSwitchService> logger)
    {
        _eventBus = eventBus;
        _configManager = configManager;
        _logger = logger;
        _configManager.StoreChanged += RebuildHotkeys;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RebuildHotkeys();
        _eventBus.OnKeyPress += OnKeyPress;
        stoppingToken.Register(() => _eventBus.OnKeyPress -= OnKeyPress);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress -= OnKeyPress;
        _configManager.StoreChanged -= RebuildHotkeys;
        return base.StopAsync(cancellationToken);
    }

    private void RebuildHotkeys()
    {
        lock (_lock)
        {
            _hotkeyToProfileId.Clear();
            foreach (var profile in _configManager.GetStore().Profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.SwitchHotkey))
                    continue;

                var key = KeyParser.Parse(profile.SwitchHotkey);
                if (key == Keys.None)
                    continue;

                _hotkeyToProfileId[key] = profile.Id;
            }
        }
    }

    private void OnKeyPress(object? sender, KeyInputEventArgs e)
    {
        string? profileId;
        lock (_lock)
        {
            _hotkeyToProfileId.TryGetValue(e.Key, out profileId);
        }

        if (profileId is null)
            return;

        try
        {
            _configManager.SetActiveProfile(profileId);
            var profile = _configManager.GetProfile(profileId);
            _logger.LogInformation("Switched to config profile {ProfileName}", profile?.Name ?? profileId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to switch config profile");
        }
    }
}
