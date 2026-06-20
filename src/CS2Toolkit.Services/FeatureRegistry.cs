using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

public sealed class FeatureRegistry : IFeatureRegistry, IHostedService
{
    private readonly IReadOnlyList<IFeatureService> _features;
    private readonly IFeatureState _state;
    private readonly IKeybindDispatcher _keybindDispatcher;
    private readonly ILogger<FeatureRegistry> _logger;

    public FeatureRegistry(
        IEnumerable<IFeatureService> features,
        IFeatureState state,
        IKeybindDispatcher keybindDispatcher,
        ILogger<FeatureRegistry> logger)
    {
        _features = features.ToList();
        _state = state;
        _keybindDispatcher = keybindDispatcher;
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
                _logger.LogWarning("Panic pressed — all combat features disabled");
                break;

            case ToolkitKeybindActions.SaveSettings:
                _logger.LogInformation("Save settings keybind pressed (persistence wiring deferred)");
                break;
        }
    }
}
