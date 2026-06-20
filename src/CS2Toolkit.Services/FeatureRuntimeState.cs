using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

internal sealed class FeatureRuntimeState : IFeatureState
{
    private readonly Dictionary<string, bool> _enabled = new(StringComparer.OrdinalIgnoreCase)
    {
        [FeatureIds.Rcs.Value] = false,
        [FeatureIds.Triggerbot.Value] = false,
        [FeatureIds.SoundEsp.Value] = false,
        [FeatureIds.AimHelper.Value] = false,
        [FeatureIds.Menu.Value] = false
    };

    public EnemyEspMode EnemyEspMode { get; private set; } = EnemyEspMode.Disabled;
    public bool TriggerbotAutoStopEnabled { get; private set; }
    public bool AimHelperActivationHeld { get; set; }

    public bool IsEnabled(FeatureId featureId) =>
        featureId.Value == FeatureIds.EnemyEsp.Value
            ? EnemyEspMode != EnemyEspMode.Disabled
            : _enabled.TryGetValue(featureId.Value, out var enabled) && enabled;

    public void SetEnabled(FeatureId featureId, bool enabled)
    {
        if (featureId.Value == FeatureIds.EnemyEsp.Value)
        {
            EnemyEspMode = enabled ? EnemyEspMode.LastSeen : EnemyEspMode.Disabled;
            return;
        }

        _enabled[featureId.Value] = enabled;
    }

    public bool Toggle(FeatureId featureId)
    {
        if (featureId.Value == FeatureIds.EnemyEsp.Value)
        {
            CycleEnemyEspMode();
            return EnemyEspMode != EnemyEspMode.Disabled;
        }

        var next = !IsEnabled(featureId);
        SetEnabled(featureId, next);
        return next;
    }

    public void CycleEnemyEspMode()
    {
        EnemyEspMode = EnemyEspMode switch
        {
            EnemyEspMode.Disabled => EnemyEspMode.LastSeen,
            EnemyEspMode.LastSeen => EnemyEspMode.Full,
            _ => EnemyEspMode.Disabled
        };
    }

    public void ToggleTriggerbotAutoStop() =>
        TriggerbotAutoStopEnabled = !TriggerbotAutoStopEnabled;

    public void DisableAllCombatFeatures()
    {
        foreach (var key in _enabled.Keys.ToList())
            _enabled[key] = false;

        EnemyEspMode = EnemyEspMode.Disabled;
        TriggerbotAutoStopEnabled = false;
        AimHelperActivationHeld = false;
    }

    public void ApplyFromProfile(ProfileSettings profile)
    {
        _enabled[FeatureIds.Triggerbot.Value] = profile.Triggerbot.Global.Enabled ?? false;
        _enabled[FeatureIds.Rcs.Value] = profile.Rcs.Global.Enabled ?? false;
        _enabled[FeatureIds.AimHelper.Value] = profile.AimHelper.Global.Enabled ?? false;
        _enabled[FeatureIds.SoundEsp.Value] = profile.SoundEsp.Enabled;
        _enabled[FeatureIds.Menu.Value] = false;
        EnemyEspMode = ParseEnemyEspMode(profile.EnemyEsp.Mode);
        TriggerbotAutoStopEnabled = profile.Triggerbot.Global.AutoStopEnabled ?? false;
        AimHelperActivationHeld = false;
    }

    private static EnemyEspMode ParseEnemyEspMode(string mode) =>
        Enum.TryParse<EnemyEspMode>(mode, ignoreCase: true, out var parsed)
            ? parsed
            : EnemyEspMode.Disabled;
}
