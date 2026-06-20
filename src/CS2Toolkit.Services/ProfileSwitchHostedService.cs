using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

internal sealed class ProfileSwitchHostedService : IHostedService
{
    private readonly IInputListener _inputListener;
    private readonly IKeybindMatcher _keybindMatcher;
    private readonly IConfigurationStore _configurationStore;
    private readonly IConfigurationChangeNotifier _changeNotifier;
    private readonly IStatusToastPublisher _statusToasts;
    private readonly ILogger<ProfileSwitchHostedService> _logger;
    private readonly object _lock = new();
    private Dictionary<int, string> _hotkeyToProfileId = new();

    public ProfileSwitchHostedService(
        IInputListener inputListener,
        IKeybindMatcher keybindMatcher,
        IConfigurationStore configurationStore,
        IConfigurationChangeNotifier changeNotifier,
        IStatusToastPublisher statusToasts,
        ILogger<ProfileSwitchHostedService> logger)
    {
        _inputListener = inputListener;
        _keybindMatcher = keybindMatcher;
        _configurationStore = configurationStore;
        _changeNotifier = changeNotifier;
        _statusToasts = statusToasts;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RebuildHotkeys();
        _changeNotifier.ConfigurationChanged += RebuildHotkeys;
        _inputListener.KeyPress += OnKeyPress;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _changeNotifier.ConfigurationChanged -= RebuildHotkeys;
        _inputListener.KeyPress -= OnKeyPress;
        return Task.CompletedTask;
    }

    private void RebuildHotkeys()
    {
        var map = new Dictionary<int, string>();
        foreach (var profile in _configurationStore.GetStore().Profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.SwitchHotkey))
                continue;

            var key = _keybindMatcher.ParseKey(profile.SwitchHotkey);
            if (key.IsNone)
                continue;

            if (map.TryGetValue(key.VirtualKey, out var existingProfileId))
            {
                _logger.LogWarning(
                    "Duplicate profile switch hotkey {Hotkey} for profiles {ExistingProfileId} and {ProfileId}; keeping {ProfileId}",
                    profile.SwitchHotkey,
                    existingProfileId,
                    profile.Id,
                    profile.Id);
            }

            map[key.VirtualKey] = profile.Id;
        }

        lock (_lock)
            _hotkeyToProfileId = map;
    }

    private void OnKeyPress(object? sender, KeyInputEvent e)
    {
        string? profileId;
        lock (_lock)
        {
            if (!_hotkeyToProfileId.TryGetValue(e.Key.VirtualKey, out profileId))
                return;
        }

        try
        {
            _configurationStore.SetActiveProfile(profileId);
            var profile = _configurationStore.GetActiveProfile();
            _statusToasts.Publish($"Profile: {profile.Name}", TimeSpan.FromSeconds(2));
            _logger.LogInformation("Switched to config profile {ProfileName} ({ProfileId})", profile.Name, profile.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to switch to profile {ProfileId}", profileId);
            _statusToasts.Publish("Failed to switch profile", TimeSpan.FromSeconds(3), 0xFFFF6B6B);
        }
    }
}
