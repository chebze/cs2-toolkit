using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class ProfileSettingsSaver
{
    private readonly IConfigurationStore _store;
    private readonly IFeatureState _state;

    public ProfileSettingsSaver(IConfigurationStore store, IFeatureState state)
    {
        _store = store;
        _state = state;
    }

    public ConfigProfile SaveActiveProfile()
    {
        var profile = _store.GetActiveProfile();

        profile.Settings.Triggerbot.Global.Enabled = _state.IsEnabled(FeatureIds.Triggerbot);
        profile.Settings.Rcs.Global.Enabled = _state.IsEnabled(FeatureIds.Rcs);
        profile.Settings.AimHelper.Global.Enabled = _state.IsEnabled(FeatureIds.AimHelper);
        profile.Settings.SoundEsp.Enabled = _state.IsEnabled(FeatureIds.SoundEsp);
        profile.Settings.Triggerbot.Global.AutoStopEnabled = _state.TriggerbotAutoStopEnabled;
        profile.Settings.EnemyEsp.Mode = _state.EnemyEspMode switch
        {
            EnemyEspMode.Full => nameof(EnemyEspMode.Full),
            EnemyEspMode.LastSeen => nameof(EnemyEspMode.LastSeen),
            _ => nameof(EnemyEspMode.Disabled)
        };

        return _store.UpdateProfile(profile);
    }
}
