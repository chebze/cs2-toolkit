namespace CS2Toolkit.Services.Abstractions;

public readonly record struct FeatureId(string Value)
{
    public override string ToString() => Value;

    public static FeatureId Parse(string value) => new(value);
}

public static class FeatureIds
{
    public static FeatureId Rcs { get; } = new("rcs");
    public static FeatureId Triggerbot { get; } = new("triggerbot");
    public static FeatureId EnemyEsp { get; } = new("enemy-esp");
    public static FeatureId SoundEsp { get; } = new("sound-esp");
    public static FeatureId AimHelper { get; } = new("aim-helper");
    public static FeatureId Menu { get; } = new("menu");
}
