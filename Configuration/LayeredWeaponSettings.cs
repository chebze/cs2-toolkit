namespace Cs2Toolkit.Configuration;

public sealed class LayeredWeaponSettings<T> where T : class, new()
{
    public T Global { get; set; } = new();
    public Dictionary<string, T> ByWeaponType { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, T> ByWeapon { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public static class WeaponSettingsResolver
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
        var json = System.Text.Json.JsonSerializer.Serialize(source);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json) ?? new T();
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

public sealed class TriggerbotLayerSettings
{
    public bool? Enabled { get; set; }
    public bool? AutoStopEnabled { get; set; }
    public float? PreFireFovDegrees { get; set; }
    public int? MinReactionDelayMs { get; set; }
    public int? MaxReactionDelayMs { get; set; }
}

public sealed class RcsLayerSettings
{
    public bool? Enabled { get; set; }
    public float? Sensitivity { get; set; }
    public float? PitchScale { get; set; }
    public float? YawScale { get; set; }
    public float? FirstBulletCompensateChance { get; set; }
    public float? SubsequentBulletSkipChance { get; set; }
}

public sealed class AimHelperLayerSettings
{
    public bool? Enabled { get; set; }
    public string? PreferredBone { get; set; }
    public float? FovDegrees { get; set; }
}
