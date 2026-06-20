using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Internal;

public sealed class GrenadeSimulationOptions
{
    public float TickIntervalSeconds { get; init; } = 1f / 64f;
    public float Gravity { get; init; } = 320f;
    public float BounceElasticity { get; init; } = 0.45f;
    public float SurfaceOffset { get; init; } = 0.25f;
    public float StopVelocityThreshold { get; init; } = 20f;
    public float MinThrowSpeedScale { get; init; } = 0.7f;
    public float MaxThrowSpeedScale { get; init; } = 0.3f;
    public float PlayerVelocityScale { get; init; } = 1.25f;
    public int MaxSimulationTicks { get; init; } = 512;
    public int MaxBounces { get; init; } = 10;
    public int MaxTrailPoints { get; init; } = 500;
    public int MaxEntityScanIndex { get; init; } = 1024;
    public int MinGameTrailPoints { get; init; } = 8;
    public int RaycastSubSteps { get; init; } = 4;
    public float RaycastSkin { get; init; } = 0.5f;
    public int RecordIntervalTicks { get; init; } = 1;
    public float MinPointSpacingUnits { get; init; } = 4f;
    public float MinTrajectoryHorizontalTravelUnits { get; init; } = 32f;
    public float ThrowForwardTraceUnits { get; init; } = 22f;
    public float ThrowStartPullbackUnits { get; init; } = 6f;
}

internal sealed class GrenadeTrajectoryDiagnostics
{
    public GrenadeTrajectorySnapshot Snapshot { get; init; } = new();
    public string Status { get; init; } = string.Empty;
}

internal sealed class GrenadeTrajectorySnapshot
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

internal enum GrenadeTrajectorySource
{
    None,
    GameTrail,
    StashedSimulation,
    ManualSimulation
}
