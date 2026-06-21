using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Runtime.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

public sealed class FeatureRegistry : IFeatureRegistry, IHostedService
{
    private readonly IReadOnlyList<IFeatureService> _features;
    private readonly IFeatureState _state;
    private readonly IKeybindDispatcher _keybindDispatcher;
    private readonly IStatusToastPublisher _statusToasts;
    private readonly ProfileSettingsSaver _settingsSaver;
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly IGameAttachment _gameAttachment;
    private readonly ILogger<FeatureRegistry> _logger;

    public FeatureRegistry(
        IEnumerable<IFeatureService> features,
        IFeatureState state,
        IKeybindDispatcher keybindDispatcher,
        IStatusToastPublisher statusToasts,
        ProfileSettingsSaver settingsSaver,
        IRuntimeOrchestrator orchestrator,
        IGameAttachment gameAttachment,
        ILogger<FeatureRegistry> logger)
    {
        _features = features.ToList();
        _state = state;
        _keybindDispatcher = keybindDispatcher;
        _statusToasts = statusToasts;
        _settingsSaver = settingsSaver;
        _orchestrator = orchestrator;
        _gameAttachment = gameAttachment;
        _logger = logger;
    }

    public IReadOnlyList<IFeatureService> Features => _features;

    public bool TryGet(FeatureId id, out IFeatureService? feature)
    {
        feature = _features.FirstOrDefault(f => f.Id == id);
        return feature is not null;
    }

    public bool IsEnabled(FeatureId id) => _state.IsEnabled(id);

    public void SetEnabled(FeatureId id, bool enabled)
    {
        _state.SetEnabled(id, enabled);
        _logger.LogInformation("Feature {FeatureId} {State}", id, enabled ? "enabled" : "disabled");
    }

    public bool Toggle(FeatureId id)
    {
        var enabled = _state.Toggle(id);
        if (id.Value == FeatureIds.EnemyEsp.Value)
            _logger.LogInformation("Enemy ESP mode set to {Mode}", _state.EnemyEspMode);
        else
            _logger.LogInformation("Feature {FeatureId} {State}", id, enabled ? "enabled" : "disabled");

        return enabled;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _keybindDispatcher.KeybindActivated += OnKeybindActivated;
        _logger.LogInformation("Feature registry ready ({Count} features)", _features.Count);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _keybindDispatcher.KeybindActivated -= OnKeybindActivated;
        return Task.CompletedTask;
    }

    private void OnKeybindActivated(object? sender, KeybindActivatedEventArgs e)
    {
        switch (e.Match.ActionId)
        {
            case ToolkitKeybindActions.RcsToggle:
                Toggle(FeatureIds.Rcs);
                break;

            case ToolkitKeybindActions.TriggerbotToggle:
                Toggle(FeatureIds.Triggerbot);
                break;

            case ToolkitKeybindActions.EnemyEspToggle:
                Toggle(FeatureIds.EnemyEsp);
                break;

            case ToolkitKeybindActions.SoundEspToggle:
                Toggle(FeatureIds.SoundEsp);
                break;

            case ToolkitKeybindActions.BulletTracersToggle:
                Toggle(FeatureIds.BulletTracers);
                break;

            case ToolkitKeybindActions.AimHelperToggle:
                Toggle(FeatureIds.AimHelper);
                break;

            case ToolkitKeybindActions.AimHelperActivation:
                _state.AimHelperActivationHeld = true;
                _logger.LogDebug("Aim helper activation pressed");
                break;

            case ToolkitKeybindActions.TriggerbotAutoStrafe:
                _state.ToggleTriggerbotAutoStop();
                _logger.LogInformation(
                    "Triggerbot auto-stop {State}",
                    _state.TriggerbotAutoStopEnabled ? "enabled" : "disabled");
                break;

            case ToolkitKeybindActions.MenuToggle:
                Toggle(FeatureIds.Menu);
                break;

            case ToolkitKeybindActions.Panic:
                _state.DisableAllCombatFeatures();
                _gameAttachment.Detach();
                _statusToasts.Publish("Panic — shutting down", TimeSpan.FromSeconds(2));
                _logger.LogWarning("Panic pressed — detaching and shutting down");
                _orchestrator.RequestShutdown("Panic key pressed");
                break;

            case ToolkitKeybindActions.SaveSettings:
                try
                {
                    var saved = _settingsSaver.SaveActiveProfile();
                    _statusToasts.Publish($"Saved profile \"{saved.Name}\"", TimeSpan.FromSeconds(2));
                    _logger.LogInformation("Saved active profile {ProfileId} to configuration store", saved.Id);
                }
                catch (Exception ex)
                {
                    _statusToasts.Publish("Failed to save settings", TimeSpan.FromSeconds(3), 0xFFFF6B6B);
                    _logger.LogError(ex, "Failed to save active profile");
                }
                break;
        }
    }
}
