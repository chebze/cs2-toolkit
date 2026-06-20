namespace CS2Toolkit.Models.Abstractions;

public sealed record RcsState(
    Vector3 AimPunch,
    int ShotsFired,
    bool IsScoped,
    bool HasAimPunch)
{
    public static RcsState Inactive { get; } = new(default, 0, false, false);
}
