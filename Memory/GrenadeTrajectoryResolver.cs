using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

public sealed class GrenadeTrajectoryResolver
{
    private readonly GrenadeTrajectorySimulator _simulator;
    private readonly GrenadeOptions _options;

    public GrenadeTrajectoryResolver(GrenadeOptions options)
    {
        _options = options;
        _simulator = new GrenadeTrajectorySimulator(options);
    }

    public GrenadeTrajectorySnapshot Resolve(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        nint clientBase,
        MemoryState state)
    {
        if (!memory.IsAttached || !state.IsInMatch || offsets.M_pWeaponServices == nint.Zero)
            return Inactive();

        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        var entityList = memory.ReadPtr(clientBase + offsets.DwEntityList);
        if (localPawn == nint.Zero || entityList == nint.Zero)
            return Inactive();

        if (!TryReadActiveGrenade(memory, offsets, entityList, localPawn, out _, out var weaponId, out var pinPulled, out var throwStrength))
            return Inactive();

        if (!pinPulled)
            return Inactive();

        if (TryReadManualSimulation(memory, offsets, mapChecker, localPawn, weaponId, throwStrength, out var manualSnapshot))
            return manualSnapshot;

        if (offsets.M_bGrenadeParametersStashed != nint.Zero
            && memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) != 0
            && TryReadStashedSimulation(memory, offsets, mapChecker, localPawn, weaponId, out var stashedSnapshot))
            return stashedSnapshot;

        if (TryReadProjectileTrail(memory, offsets, entityList, out var projectileTrail)
            && IsRichGameTrail(projectileTrail)
            && TryResimulateFromTrail(mapChecker, projectileTrail, weaponId, out var trailSnapshot))
            return trailSnapshot;

        return Inactive();
    }

    private bool TryReadStashedSimulation(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        nint localPawn,
        ushort weaponId,
        out GrenadeTrajectorySnapshot snapshot)
    {
        snapshot = Inactive();

        if (offsets.M_vecStashedGrenadeThrowPosition == nint.Zero || offsets.M_vecStashedVelocity == nint.Zero)
            return false;

        var start = ReadNumericVector(memory, localPawn + offsets.M_vecStashedGrenadeThrowPosition);
        var velocity = ReadNumericVector(memory, localPawn + offsets.M_vecStashedVelocity);
        if (!IsUsableVector(start) || velocity.LengthSquared() < 10_000f)
            return false;

        if (!_simulator.TrySimulate(mapChecker, start, velocity, out var points, out var landing, out var bounces))
            return false;

        if (!GrenadeTrajectorySimulator.IsPlausibleTrajectory(points, _options.MinTrajectoryHorizontalTravelUnits))
            return false;

        snapshot = BuildSnapshot(GrenadeTrajectorySource.StashedSimulation, weaponId, points, landing, bounces);
        return true;
    }

    private bool TryReadManualSimulation(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        nint localPawn,
        ushort weaponId,
        float throwStrength,
        out GrenadeTrajectorySnapshot snapshot)
    {
        snapshot = Inactive();

        if (!TryReadThrowAngles(memory, offsets, localPawn, out var pitch, out var yaw))
            return false;

        if (!TryReadEyePosition(memory, offsets, localPawn, out var eyePosition))
            return false;

        var playerVelocity = offsets.M_vecAbsVelocity != nint.Zero
            ? ReadNumericVector(memory, localPawn + offsets.M_vecAbsVelocity)
            : System.Numerics.Vector3.Zero;

        if (!GrenadeTrajectorySimulator.TryComputeThrowState(
                mapChecker,
                ToNumeric(eyePosition),
                pitch,
                yaw,
                throwStrength,
                weaponId,
                playerVelocity,
                _options,
                out var start,
                out var throwVelocity))
            return false;

        if (!_simulator.TrySimulate(mapChecker, start, throwVelocity, out var points, out var landing, out var bounces))
            return false;

        if (!GrenadeTrajectorySimulator.IsPlausibleTrajectory(points, _options.MinTrajectoryHorizontalTravelUnits))
            return false;

        snapshot = BuildSnapshot(GrenadeTrajectorySource.ManualSimulation, weaponId, points, landing, bounces);
        return true;
    }

    private bool TryResimulateFromTrail(
        MapVisibilityChecker mapChecker,
        IReadOnlyList<Vector3> trail,
        ushort weaponId,
        out GrenadeTrajectorySnapshot snapshot)
    {
        snapshot = Inactive();

        if (trail.Count < 2)
            return false;

        var start = ToNumeric(trail[0]);
        var dt = Math.Max(_options.TickIntervalSeconds, 1e-4f);
        var velocity = (ToNumeric(trail[1]) - start) / dt;
        if (velocity.LengthSquared() < 10_000f && trail.Count >= 3)
            velocity = (ToNumeric(trail[2]) - start) / (dt * 2f);

        if (velocity.LengthSquared() < 10_000f)
            return false;

        if (!_simulator.TrySimulate(mapChecker, start, velocity, out var points, out var landing, out var bounces))
            return false;

        if (!GrenadeTrajectorySimulator.IsPlausibleTrajectory(points, _options.MinTrajectoryHorizontalTravelUnits))
            return false;

        snapshot = BuildSnapshot(GrenadeTrajectorySource.GameTrail, weaponId, points, landing, bounces);
        return true;
    }

    private bool TryReadThrowAngles(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        out float pitch,
        out float yaw)
    {
        pitch = 0f;
        yaw = 0f;

        if (offsets.M_bGrenadeParametersStashed != nint.Zero
            && memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) != 0
            && offsets.M_angStashedShootAngles != nint.Zero)
        {
            pitch = memory.Read<float>(localPawn + offsets.M_angStashedShootAngles);
            yaw = memory.Read<float>(localPawn + offsets.M_angStashedShootAngles + 4);
            return true;
        }

        if (offsets.M_angEyeAngles == nint.Zero)
            return false;

        pitch = memory.Read<float>(localPawn + offsets.M_angEyeAngles);
        yaw = memory.Read<float>(localPawn + offsets.M_angEyeAngles + 4);
        return true;
    }

    private bool TryReadProjectileTrail(
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        out List<Vector3> points)
    {
        points = [];

        if (offsets.M_arrTrajectoryTrailPoints == nint.Zero || offsets.DwGameEntitySystemHighestEntityIndex == nint.Zero)
            return false;

        var highestIndex = memory.Read<int>(memory.ClientBase + offsets.DwGameEntitySystemHighestEntityIndex);
        highestIndex = Math.Clamp(highestIndex, 0, _options.MaxEntityScanIndex);

        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            for (var index = 64; index <= highestIndex; index++)
            {
                var entity = ResolveEntityFromIndex(memory, entityList, index, spacing);
                if (entity == nint.Zero)
                    continue;

                var scratch = new List<Vector3>();
                if (!TryReadVector3Array(memory, entity + offsets.M_arrTrajectoryTrailPoints, _options.MaxTrailPoints, scratch))
                    continue;

                if (scratch.Count > points.Count)
                {
                    points.Clear();
                    points.AddRange(scratch);
                }
            }

            if (points.Count >= _options.MinGameTrailPoints)
                return true;
        }

        points.Clear();
        return false;
    }

    private bool IsRichGameTrail(IReadOnlyList<Vector3> points)
    {
        if (points.Count < _options.MinGameTrailPoints)
            return false;

        var totalLength = 0f;
        for (var i = 1; i < points.Count; i++)
            totalLength += SegmentLength(points[i - 1], points[i]);

        if (totalLength < 64f)
            return false;

        var start = points[0];
        var end = points[^1];
        var chord = SegmentLength(start, end);
        if (chord <= 1f)
            return true;

        var maxDeviation = 0f;
        for (var i = 1; i < points.Count - 1; i++)
            maxDeviation = MathF.Max(maxDeviation, DistancePointToSegment(points[i], start, end));

        return maxDeviation >= 4f || totalLength >= chord * 1.15f;
    }

    private static float SegmentLength(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static float DistancePointToSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        var abX = end.X - start.X;
        var abY = end.Y - start.Y;
        var abZ = end.Z - start.Z;
        var abLenSq = abX * abX + abY * abY + abZ * abZ;
        if (abLenSq <= 1e-4f)
            return SegmentLength(point, start);

        var apX = point.X - start.X;
        var apY = point.Y - start.Y;
        var apZ = point.Z - start.Z;
        var t = Math.Clamp((apX * abX + apY * abY + apZ * abZ) / abLenSq, 0f, 1f);

        var closestX = start.X + abX * t;
        var closestY = start.Y + abY * t;
        var closestZ = start.Z + abZ * t;
        var dx = point.X - closestX;
        var dy = point.Y - closestY;
        var dz = point.Z - closestZ;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static bool TryReadActiveGrenade(
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint localPawn,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength)
    {
        weapon = nint.Zero;
        weaponId = 0;
        pinPulled = false;
        throwStrength = 0f;

        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = memory.Read<uint>(weaponServices + offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        weapon = ResolveEntityFromHandle(memory, entityList, activeHandle);
        if (weapon == nint.Zero)
            return false;

        weaponId = memory.Read<ushort>(weapon + offsets.M_AttributeManager + offsets.M_Item + offsets.M_iItemDefinitionIndex);
        if (!GameOffsets.IsGrenadeWeapon(weaponId))
            return false;

        if (offsets.M_bPinPulled != nint.Zero)
            pinPulled = memory.Read<byte>(weapon + offsets.M_bPinPulled) != 0;

        if (offsets.M_flThrowStrength != nint.Zero)
            throwStrength = memory.Read<float>(weapon + offsets.M_flThrowStrength);

        return true;
    }

    private static bool TryReadEyePosition(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        out Vector3 eyePosition)
    {
        eyePosition = BombSiteHelper.ReadEntityPosition(memory, offsets, localPawn);
        if (!eyePosition.IsValid)
            return false;

        if (offsets.M_vecViewOffset == nint.Zero)
            return true;

        var viewOffset = ReadModelVector(memory, localPawn + offsets.M_vecViewOffset);
        eyePosition = new Vector3(
            eyePosition.X + viewOffset.X,
            eyePosition.Y + viewOffset.Y,
            eyePosition.Z + viewOffset.Z);

        return true;
    }

    private static bool TryReadVector3Array(
        ProcessMemory memory,
        nint vectorAddress,
        int maxCount,
        List<Vector3> output)
    {
        output.Clear();

        var count = memory.Read<int>(vectorAddress);
        if (count <= 1 || count > maxCount)
            return false;

        var dataPtr = memory.ReadPtr(vectorAddress + 0x8);
        if (dataPtr == nint.Zero)
            return false;

        for (var i = 0; i < count; i++)
        {
            var address = dataPtr + (nint)(i * 12);
            var point = ReadModelVector(memory, address);
            if (!point.IsValid)
                return false;

            output.Add(point);
        }

        return output.Count >= 2;
    }

    private static GrenadeTrajectorySnapshot BuildSnapshot(
        GrenadeTrajectorySource source,
        ushort weaponId,
        IReadOnlyList<Vector3> points,
        Vector3 landingPoint,
        int bounceCount) =>
        new()
        {
            IsActive = points.Count >= 2,
            Source = source,
            WeaponId = weaponId,
            Points = points,
            LandingPoint = landingPoint,
            BounceCount = bounceCount
        };

    private static GrenadeTrajectorySnapshot Inactive() =>
        new() { IsActive = false, Source = GrenadeTrajectorySource.None };

    private static bool IsUsableVector(System.Numerics.Vector3 vector) =>
        IsFinite(vector) && (MathF.Abs(vector.X) > 1f || MathF.Abs(vector.Y) > 1f || MathF.Abs(vector.Z) > 1f);

    private static Vector3 ReadModelVector(ProcessMemory memory, nint address) =>
        new(
            memory.Read<float>(address),
            memory.Read<float>(address + 4),
            memory.Read<float>(address + 8));

    private static System.Numerics.Vector3 ReadNumericVector(ProcessMemory memory, nint address) =>
        new(
            memory.Read<float>(address),
            memory.Read<float>(address + 4),
            memory.Read<float>(address + 8));

    private static System.Numerics.Vector3 ToNumeric(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    private static bool IsFinite(System.Numerics.Vector3 vector) =>
        !float.IsNaN(vector.X) && !float.IsNaN(vector.Y) && !float.IsNaN(vector.Z)
        && !float.IsInfinity(vector.X) && !float.IsInfinity(vector.Y) && !float.IsInfinity(vector.Z);

    private static nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolveEntityFromHandle(memory, entityList, handle, spacing);
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }

    private static nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle, int spacing)
    {
        var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((handle & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return memory.ReadPtr(listEntry + (nint)(spacing * (handle & 0x1FF)));
    }

    private static nint ResolveEntityFromIndex(ProcessMemory memory, nint entityList, int index, int spacing)
    {
        var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
    }
}
