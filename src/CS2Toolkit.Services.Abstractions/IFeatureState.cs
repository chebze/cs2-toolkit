using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public interface IFeatureState
{
    bool IsEnabled(FeatureId featureId);
    void SetEnabled(FeatureId featureId, bool enabled);
    bool Toggle(FeatureId featureId);
    EnemyEspMode EnemyEspMode { get; }
    void CycleEnemyEspMode();
    bool TriggerbotAutoStopEnabled { get; }
    void ToggleTriggerbotAutoStop();
    bool AimHelperActivationHeld { get; set; }
    void DisableAllCombatFeatures();
    void ApplyFromProfile(ProfileSettings profile);
}
