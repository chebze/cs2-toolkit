using CS2Toolkit.Configuration.Abstractions;

namespace CS2Toolkit.Configuration;

public sealed class SettingsResolver : ISettingsResolver
{
    public ResolvedWeaponSettings ResolveWeaponSettings(ProfileSettings profile, ushort weaponId) => new()
    {
        Triggerbot = ResolveTriggerbot(profile, weaponId),
        Rcs = ResolveRcs(profile, weaponId),
        AimHelper = ResolveAimHelper(profile, weaponId)
    };

    public TriggerbotLayerSettings ResolveTriggerbot(ProfileSettings profile, ushort weaponId) =>
        WeaponSettingsResolver.Resolve(profile.Triggerbot, weaponId);

    public RcsLayerSettings ResolveRcs(ProfileSettings profile, ushort weaponId) =>
        WeaponSettingsResolver.Resolve(profile.Rcs, weaponId);

    public AimHelperLayerSettings ResolveAimHelper(ProfileSettings profile, ushort weaponId) =>
        WeaponSettingsResolver.Resolve(profile.AimHelper, weaponId);
}

internal static class WeaponSettingsResolver
{
    public static T Resolve<T>(LayeredWeaponSettings<T> layered, ushort weaponId) where T : class, new()
    {
        var result = Clone(layered.Global);
        var category = WeaponCatalog.CategoryKey(WeaponCatalog.GetCategory(weaponId));

        if (layered.ByWeaponType.TryGetValue(category, out var typeSettings) && typeSettings is not null)
            Merge(result, typeSettings);

        var weaponKey = weaponId.ToString();
        if (layered.ByWeapon.TryGetValue(weaponKey, out var weaponSettings) && weaponSettings is not null)
            Merge(result, weaponSettings);

        return result;
    }

    private static T Clone<T>(T source) where T : class, new()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(source, ConfigurationJson.Options);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, ConfigurationJson.Options) ?? new T();
    }

    private static void Merge<T>(T target, T overlay) where T : class
    {
        foreach (var property in typeof(T).GetProperties())
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            var value = property.GetValue(overlay);
            if (value is null)
                continue;

            property.SetValue(target, value);
        }
    }
}
