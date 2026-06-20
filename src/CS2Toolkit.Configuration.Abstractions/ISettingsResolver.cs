namespace CS2Toolkit.Configuration.Abstractions;

public interface ISettingsResolver
{
    ResolvedWeaponSettings ResolveWeaponSettings(ProfileSettings profile, ushort weaponId);
    TriggerbotLayerSettings ResolveTriggerbot(ProfileSettings profile, ushort weaponId);
    RcsLayerSettings ResolveRcs(ProfileSettings profile, ushort weaponId);
    AimHelperLayerSettings ResolveAimHelper(ProfileSettings profile, ushort weaponId);
}
