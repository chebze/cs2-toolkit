using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Game.Memory;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;
using InternalGrenadeSource = CS2Toolkit.Game.Internal.GrenadeTrajectorySource;

namespace CS2Toolkit.Game.Mapping;

internal static class GrenadeSimulationOptionsFactory
{
    public static GrenadeSimulationOptions FromSettings(GrenadePhysicsSettings settings) => new()
    {
        TickIntervalSeconds = settings.TickIntervalSeconds,
        Gravity = settings.Gravity,
        BounceElasticity = settings.BounceElasticity,
        SurfaceOffset = settings.SurfaceOffset,
        StopVelocityThreshold = settings.StopVelocityThreshold,
        MaxSimulationTicks = settings.MaxSimulationTicks,
        MaxBounces = settings.MaxBounces,
        MinThrowSpeedScale = settings.MinThrowSpeedScale,
        MaxThrowSpeedScale = settings.MaxThrowSpeedScale,
        PlayerVelocityScale = settings.PlayerVelocityScale,
        RaycastSubSteps = settings.RaycastSubSteps,
        RaycastSkin = settings.RaycastSkin,
        MinPointSpacingUnits = settings.MinPointSpacingUnits,
        ThrowForwardTraceUnits = settings.ThrowForwardTraceUnits,
        ThrowStartPullbackUnits = settings.ThrowStartPullbackUnits,
        MaxEntityScanIndex = settings.MaxEntityScanIndex
    };
}

internal sealed class GrenadeTrajectoryReader
{
    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly MapVisibilityChecker _mapChecker;
    private readonly GrenadeTrajectoryResolver _resolver;

    public GrenadeTrajectoryReader(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        GrenadeSimulationOptions options)
    {
        _memory = memory;
        _offsets = offsets;
        _mapChecker = mapChecker;
        _resolver = new GrenadeTrajectoryResolver(options);
    }

    public GrenadeState Read(bool isInMatch)
    {
        if (!_memory.IsAttached)
            return GrenadeState.Inactive;

        var result = _resolver.Resolve(
            _memory,
            _offsets,
            _mapChecker,
            _memory.ClientBase,
            isInMatch);

        return MapState(result.Snapshot);
    }

    private static GrenadeState MapState(GrenadeTrajectorySnapshot snapshot)
    {
        if (!snapshot.IsActive || snapshot.Points.Count < 2)
            return GrenadeState.Inactive;

        return new GrenadeState(
            true,
            MapSource(snapshot.Source),
            new WeaponId(snapshot.WeaponId),
            snapshot.Points,
            snapshot.Segments,
            snapshot.BouncePoints,
            snapshot.LandingPoint,
            snapshot.BounceCount);
    }

    private static Models.Abstractions.GrenadeTrajectorySource MapSource(InternalGrenadeSource source) => source switch
    {
        InternalGrenadeSource.GameTrail => Models.Abstractions.GrenadeTrajectorySource.GameTrail,
        InternalGrenadeSource.StashedSimulation => Models.Abstractions.GrenadeTrajectorySource.StashedSimulation,
        InternalGrenadeSource.ManualSimulation => Models.Abstractions.GrenadeTrajectorySource.ManualSimulation,
        _ => Models.Abstractions.GrenadeTrajectorySource.None
    };
}
