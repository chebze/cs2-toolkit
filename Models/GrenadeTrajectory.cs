namespace Cs2Toolkit.Models;

public enum GrenadeTrajectorySource
{
    None,
    GameTrail,
    StashedSimulation,
    ManualSimulation
}

public sealed class GrenadeTrajectorySnapshot
{
    public bool IsActive { get; init; }
    public GrenadeTrajectorySource Source { get; init; }
    public ushort WeaponId { get; init; }
    public IReadOnlyList<Vector3> Points { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<Vector3>> Segments { get; init; } = [];
    public IReadOnlyList<Vector3> BouncePoints { get; init; } = [];
    public Vector3 LandingPoint { get; init; }
    public int BounceCount { get; init; }
}
