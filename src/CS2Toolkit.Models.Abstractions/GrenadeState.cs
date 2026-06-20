namespace CS2Toolkit.Models.Abstractions;

public enum GrenadeTrajectorySource
{
    None,
    GameTrail,
    StashedSimulation,
    ManualSimulation
}

public sealed record GrenadeState(
    bool IsActive,
    GrenadeTrajectorySource Source,
    WeaponId WeaponId,
    IReadOnlyList<Vector3> Points,
    IReadOnlyList<IReadOnlyList<Vector3>> Segments,
    IReadOnlyList<Vector3> BouncePoints,
    Vector3 LandingPoint,
    int BounceCount)
{
    public static GrenadeState Inactive { get; } = new(
        false,
        GrenadeTrajectorySource.None,
        new WeaponId(0),
        [],
        [],
        [],
        default,
        0);
}
