namespace CS2Toolkit.Configuration.Abstractions;

public sealed class LayeredWeaponSettings<T> where T : class, new()
{
    public T Global { get; set; } = new();
    public Dictionary<string, T> ByWeaponType { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, T> ByWeapon { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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
