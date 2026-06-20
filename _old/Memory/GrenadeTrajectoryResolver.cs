using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

public sealed class GrenadeTrajectoryResolver
{
    private const float MinThrowSpeedSquared = 10_000f;
    private readonly GrenadeTrajectorySimulator _simulator;
    private readonly GrenadeOptions _options;

    public GrenadeTrajectoryResolver(GrenadeOptions options)
    {
        _options = options;
        _simulator = new GrenadeTrajectorySimulator(options);
    }

    public GrenadeTrajectoryDiagnostics Resolve(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        nint clientBase,
        MemoryState state)
    {
        if (!memory.IsAttached)
            return Inactive("memory not attached");

        if (!state.IsInMatch)
            return Inactive("not in match");

        if (offsets.M_pWeaponServices == nint.Zero)
            return Inactive("weapon services offset missing");

        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        var entityList = memory.ReadPtr(clientBase + offsets.DwEntityList);
        if (localPawn == nint.Zero || entityList == nint.Zero)
            return Inactive("local pawn or entity list unavailable");

        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return Inactive("weapon services unavailable");

        if (!TryReadGrenadeFromHandle(
                memory,
                offsets,
                entityList,
                weaponServices,
                offsets.M_hActiveWeapon,
                out _,
                out var weaponId,
                out var pinPulled,
                out var throwStrength))
        {
            var activeHandle = memory.Read<uint>(weaponServices + offsets.M_hActiveWeapon);
            return Inactive($"no active grenade (active=0x{activeHandle:X})");
        }

        throwStrength = NormalizeThrowStrength(throwStrength);

        if (!pinPulled)
            return Inactive($"active grenade id={weaponId} pin not pulled");

        if (!TryReadThrowAngles(memory, offsets, localPawn, preferStashed: false, out var pitch, out var yaw))
            return Inactive($"active grenade id={weaponId} but throw angles unavailable");

        if (TryBuildSimulation(
                memory,
                offsets,
                mapChecker,
                localPawn,
                weaponId,
                throwStrength,
                pitch,
                yaw,
                out var snapshot,
                out var buildStatus))
            return Active(snapshot, $"id={weaponId} pin={pinPulled} strength={throwStrength:F2} maps={mapChecker.LoadedMapCount} {buildStatus}");

        return Inactive($"id={weaponId} pin={pinPulled} strength={throwStrength:F2} pitch={pitch:F1} yaw={yaw:F1} maps={mapChecker.LoadedMapCount} simulation failed");
    }

    public bool IsGrenadeAimLikely(
        ProcessMemory memory,
        GameOffsets offsets,
        nint clientBase,
        MemoryState state)
    {
        if (!memory.IsAttached || !state.IsInMatch || offsets.M_pWeaponServices == nint.Zero)
            return false;

        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        var entityList = memory.ReadPtr(clientBase + offsets.DwEntityList);
        if (localPawn == nint.Zero || entityList == nint.Zero)
            return false;

        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        return TryReadGrenadeFromHandle(
            memory,
            offsets,
            entityList,
            weaponServices,
            offsets.M_hActiveWeapon,
            out _,
            out _,
            out _,
            out _);
    }

    public bool TryGetPreferredGrenadeId(
        ProcessMemory memory,
        GameOffsets offsets,
        nint clientBase,
        out ushort weaponId)
    {
        weaponId = 0;

        if (!memory.IsAttached)
            return false;

        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        var entityList = memory.ReadPtr(clientBase + offsets.DwEntityList);
        if (localPawn == nint.Zero || entityList == nint.Zero)
            return false;

        return TryGetPreferredGrenadeId(memory, offsets, entityList, localPawn, out weaponId);
    }

    public GrenadeTrajectoryDiagnostics ResolveAimPreview(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        nint clientBase,
        MemoryState state,
        ushort weaponId)
    {
        if (!memory.IsAttached)
            return Inactive("preview memory not attached");

        if (!state.IsInMatch)
            return Inactive("preview not in match");

        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        if (localPawn == nint.Zero)
            return Inactive("preview local pawn unavailable");

        if (!TryReadThrowAngles(memory, offsets, localPawn, preferStashed: false, out var pitch, out var yaw))
            return Inactive("preview throw angles unavailable");

        if (TryBuildSimulation(
                memory,
                offsets,
                mapChecker,
                localPawn,
                weaponId,
                1f,
                pitch,
                yaw,
                out var snapshot,
                out var buildStatus))
            return Active(snapshot, $"preview id={weaponId} maps={mapChecker.LoadedMapCount} {buildStatus}");

        return Inactive($"preview id={weaponId} maps={mapChecker.LoadedMapCount} simulation failed");
    }

    private static bool TryGetPreferredGrenadeId(
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint localPawn,
        out ushort weaponId)
    {
        weaponId = 0;

        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        if (TryReadGrenadeFromHandle(memory, offsets, entityList, weaponServices, offsets.M_hActiveWeapon, out _, out weaponId, out _, out _))
            return true;

        if (TryFindGrenadeInInventory(memory, offsets, entityList, weaponServices, preferPinPulled: true, out _, out weaponId, out _, out _))
            return true;

        return TryFindGrenadeInInventory(memory, offsets, entityList, weaponServices, preferPinPulled: false, out _, out weaponId, out _, out _);
    }

    private static GrenadeTrajectoryDiagnostics Active(GrenadeTrajectorySnapshot snapshot, string status) =>
        new() { Snapshot = snapshot, Status = status };

    private static GrenadeTrajectoryDiagnostics Inactive(string status) =>
        new() { Snapshot = new GrenadeTrajectorySnapshot(), Status = status };

    private bool TryBuildSimulation(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        nint localPawn,
        ushort weaponId,
        float throwStrength,
        float pitch,
        float yaw,
        out GrenadeTrajectorySnapshot snapshot,
        out string status)
    {
        snapshot = new GrenadeTrajectorySnapshot();
        status = string.Empty;

        var playerVelocity = offsets.M_vecAbsVelocity != nint.Zero
            ? ReadNumericVector(memory, localPawn + offsets.M_vecAbsVelocity)
            : System.Numerics.Vector3.Zero;

        System.Numerics.Vector3 start;
        System.Numerics.Vector3 velocity;
        const GrenadeTrajectorySource source = GrenadeTrajectorySource.ManualSimulation;

        if (!TryReadEyePosition(memory, offsets, localPawn, out var eyePosition))
        {
            status = "eye position unavailable";
            return false;
        }

        if (!GrenadeTrajectorySimulator.TryComputeThrowState(
                ToNumeric(eyePosition),
                pitch,
                yaw,
                throwStrength,
                weaponId,
                playerVelocity,
                _options,
                out start,
                out velocity))
        {
            status = "throw state compute failed";
            return false;
        }

        var origin = $"eye vel={velocity.Length():F0}";

        if (!_simulator.TrySimulate(
                mapChecker,
                start,
                velocity,
                out var points,
                out var segments,
                out var bouncePoints,
                out var landing,
                out var bounces))
        {
            status = $"{origin} simulate failed (raycast may be missing active map mesh)";
            return false;
        }

        if (points.Count < 2)
        {
            status = $"{origin} produced {points.Count} points";
            return false;
        }

        snapshot = BuildSnapshot(source, weaponId, points, segments, bouncePoints, landing, bounces);
        status = $"{origin} source={source} points={points.Count} bounces={bounces} landing=({landing.X:F0},{landing.Y:F0},{landing.Z:F0})";
        return true;
    }

    private bool TryReadStashedThrow(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        out System.Numerics.Vector3 start,
        out System.Numerics.Vector3 velocity)
    {
        start = default;
        velocity = default;

        if (offsets.M_bGrenadeParametersStashed == nint.Zero
            || memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) == 0
            || offsets.M_vecStashedGrenadeThrowPosition == nint.Zero
            || offsets.M_vecStashedVelocity == nint.Zero)
            return false;

        start = ReadNumericVector(memory, localPawn + offsets.M_vecStashedGrenadeThrowPosition);
        velocity = ReadNumericVector(memory, localPawn + offsets.M_vecStashedVelocity);

        return IsUsableVector(start) && velocity.LengthSquared() >= MinThrowSpeedSquared;
    }

    private static bool TryReadLooseStashedThrowPosition(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        out System.Numerics.Vector3 position)
    {
        position = default;

        if (offsets.M_vecStashedGrenadeThrowPosition == nint.Zero)
            return false;

        position = ReadNumericVector(memory, localPawn + offsets.M_vecStashedGrenadeThrowPosition);
        return IsUsableVector(position);
    }

    private static bool TryReadStashedThrowPosition(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        out System.Numerics.Vector3 position)
    {
        position = default;

        if (offsets.M_bGrenadeParametersStashed == nint.Zero
            || memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) == 0
            || offsets.M_vecStashedGrenadeThrowPosition == nint.Zero)
            return false;

        position = ReadNumericVector(memory, localPawn + offsets.M_vecStashedGrenadeThrowPosition);
        return IsUsableVector(position);
    }

    private bool TryResimulateFromTrail(
        MapVisibilityChecker mapChecker,
        IReadOnlyList<Vector3> trail,
        ushort weaponId,
        out GrenadeTrajectorySnapshot snapshot,
        out string status)
    {
        snapshot = new GrenadeTrajectorySnapshot();
        status = string.Empty;

        if (trail.Count < 2)
            return false;

        var start = ToNumeric(trail[0]);
        var dt = Math.Max(_options.TickIntervalSeconds, 1e-4f);
        var velocity = (ToNumeric(trail[1]) - start) / dt;
        if (velocity.LengthSquared() < MinThrowSpeedSquared && trail.Count >= 3)
            velocity = (ToNumeric(trail[2]) - start) / (dt * 2f);

        if (velocity.LengthSquared() < MinThrowSpeedSquared)
        {
            status = "trail velocity too low";
            return false;
        }

        if (!_simulator.TrySimulate(
                mapChecker,
                start,
                velocity,
                out var points,
                out var segments,
                out var bouncePoints,
                out var landing,
                out var bounces))
        {
            status = "trail resimulate failed";
            return false;
        }

        snapshot = BuildSnapshot(GrenadeTrajectorySource.GameTrail, weaponId, points, segments, bouncePoints, landing, bounces);
        status = $"points={points.Count} bounces={bounces}";
        return true;
    }

    private static bool TryReadThrowAngles(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        bool preferStashed,
        out float pitch,
        out float yaw)
    {
        pitch = 0f;
        yaw = 0f;

        if (preferStashed && offsets.M_angStashedShootAngles != nint.Zero)
        {
            pitch = memory.Read<float>(localPawn + offsets.M_angStashedShootAngles);
            yaw = memory.Read<float>(localPawn + offsets.M_angStashedShootAngles + 4);
            if (IsFinite(pitch) && IsFinite(yaw))
                return true;
        }

        return TryReadEyeAngles(memory, offsets, localPawn, out pitch, out yaw);
    }

    private static bool TryReadEyeAngles(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn,
        out float pitch,
        out float yaw)
    {
        pitch = 0f;
        yaw = 0f;

        if (offsets.M_angEyeAngles == nint.Zero)
            return false;

        pitch = memory.Read<float>(localPawn + offsets.M_angEyeAngles);
        yaw = memory.Read<float>(localPawn + offsets.M_angEyeAngles + 4);
        return IsFinite(pitch) && IsFinite(yaw);
    }

    private static bool IsFinite(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value);

    private static float NormalizeThrowStrength(float throwStrength) =>
        throwStrength <= 0.01f ? 1f : Math.Clamp(throwStrength, 0f, 1f);

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

    private static bool TryResolveGrenadeContext(
        GrenadeTrajectoryResolver resolver,
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint clientBase,
        nint localPawn,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength,
        out string detectSource) =>
        TryFindHeldGrenadeViaEntityScan(resolver._options.MaxEntityScanIndex, memory, offsets, entityList, clientBase, out weapon, out weaponId, out pinPulled, out throwStrength, out detectSource)
        || TryResolveGrenadeWeapon(memory, offsets, entityList, localPawn, out weapon, out weaponId, out pinPulled, out throwStrength, out detectSource)
        || TryResolveFromStashedPawn(resolver._options.MaxEntityScanIndex, memory, offsets, entityList, clientBase, localPawn, out weapon, out weaponId, out pinPulled, out throwStrength, out detectSource);

    private static bool TryResolveFromStashedPawn(
        int maxEntityScanIndex,
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint clientBase,
        nint localPawn,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength,
        out string detectSource)
    {
        weapon = nint.Zero;
        weaponId = 0;
        pinPulled = false;
        throwStrength = 1f;
        detectSource = string.Empty;

        var stashFlag = offsets.M_bGrenadeParametersStashed != nint.Zero
            && memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) != 0;

        if (!stashFlag && !HasUsableStashedThrowPosition(memory, offsets, localPawn))
            return false;

        pinPulled = stashFlag || HasUsableStashedThrowPosition(memory, offsets, localPawn);

        if (TryFindHeldGrenadeViaEntityScan(maxEntityScanIndex, memory, offsets, entityList, clientBase, out weapon, out weaponId, out _, out throwStrength, out _))
        {
            detectSource = "stash+held";
            return true;
        }

        if (TryGetPreferredGrenadeId(memory, offsets, entityList, localPawn, out weaponId))
        {
            detectSource = stashFlag ? "stash+inventory" : "stash-pos";
            return true;
        }

        weaponId = GameOffsets.WeaponHeGrenade;
        detectSource = stashFlag ? "stash" : "stash-pos";
        return true;
    }

    private static bool HasUsableStashedThrowPosition(
        ProcessMemory memory,
        GameOffsets offsets,
        nint localPawn)
    {
        if (offsets.M_vecStashedGrenadeThrowPosition == nint.Zero)
            return false;

        var position = ReadNumericVector(memory, localPawn + offsets.M_vecStashedGrenadeThrowPosition);
        return IsUsableVector(position);
    }

    private static string DescribeDetectionFailure(
        int maxEntityScanIndex,
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint clientBase,
        nint localPawn)
    {
        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        var activeHandle = weaponServices != nint.Zero
            ? memory.Read<uint>(weaponServices + offsets.M_hActiveWeapon)
            : 0u;
        var inventoryHandles = weaponServices != nint.Zero
            ? CollectWeaponHandles(memory, weaponServices, offsets.M_hMyWeapons).Count
            : 0;
        var stashFlag = offsets.M_bGrenadeParametersStashed != nint.Zero
            && memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) != 0;
        var stashPos = HasUsableStashedThrowPosition(memory, offsets, localPawn);
        var heldEntity = TryFindHeldGrenadeViaEntityScan(
            maxEntityScanIndex,
            memory,
            offsets,
            entityList,
            clientBase,
            out _,
            out _,
            out _,
            out _,
            out _);

        return $"held={heldEntity} services=0x{weaponServices:X} active=0x{activeHandle:X} inventory={inventoryHandles} stash={stashFlag} stashPos={stashPos}";
    }

    private static bool TryResolveGrenadeWeapon(
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint localPawn,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength,
        out string detectSource)
    {
        weapon = nint.Zero;
        weaponId = 0;
        pinPulled = false;
        throwStrength = 0f;
        detectSource = string.Empty;

        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        if (TryReadGrenadeFromHandle(memory, offsets, entityList, weaponServices, offsets.M_hActiveWeapon, out weapon, out weaponId, out pinPulled, out throwStrength))
        {
            detectSource = "active";
            return true;
        }

        if (TryFindGrenadeInInventory(memory, offsets, entityList, weaponServices, preferPinPulled: true, out weapon, out weaponId, out pinPulled, out throwStrength))
        {
            detectSource = pinPulled ? "inventory-pin" : "inventory";
            return true;
        }

        var stashActive = offsets.M_bGrenadeParametersStashed != nint.Zero
            && memory.Read<byte>(localPawn + offsets.M_bGrenadeParametersStashed) != 0;

        if (stashActive)
        {
            if (TryFindGrenadeInInventory(memory, offsets, entityList, weaponServices, preferPinPulled: false, out weapon, out weaponId, out pinPulled, out throwStrength))
            {
                detectSource = "stash+inventory";
                pinPulled = true;
                return true;
            }

            if (TryReadLooseStashedThrowPosition(memory, offsets, localPawn, out _))
            {
                weaponId = GameOffsets.WeaponHeGrenade;
                pinPulled = true;
                throwStrength = 1f;
                detectSource = "stash-pos";
                return true;
            }
        }

        if (TryFindGrenadeInInventory(memory, offsets, entityList, weaponServices, preferPinPulled: false, out weapon, out weaponId, out pinPulled, out throwStrength))
        {
            detectSource = "inventory";
            return true;
        }

        return false;
    }

    private static bool TryReadGrenadeFromHandle(
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint weaponServices,
        nint handleOffset,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength)
    {
        weapon = nint.Zero;
        weaponId = 0;
        pinPulled = false;
        throwStrength = 0f;

        var handle = memory.Read<uint>(weaponServices + handleOffset);
        if (handle is 0 or 0xFFFFFFFF)
            return false;

        weapon = ResolveEntityFromHandle(memory, entityList, handle);
        if (weapon == nint.Zero)
            return false;

        weaponId = ReadWeaponDefinitionIndex(memory, offsets, weapon);
        if (!GameOffsets.IsGrenadeWeapon(weaponId))
            return false;

        ReadGrenadeState(memory, offsets, weapon, out pinPulled, out throwStrength);
        return true;
    }

    private static bool TryFindGrenadeInInventory(
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint weaponServices,
        bool preferPinPulled,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength)
    {
        weapon = nint.Zero;
        weaponId = 0;
        pinPulled = false;
        throwStrength = 0f;

        nint fallbackWeapon = nint.Zero;
        ushort fallbackWeaponId = 0;
        var fallbackPinPulled = false;
        var fallbackThrowStrength = 0f;

        foreach (var handle in CollectWeaponHandles(memory, weaponServices, offsets.M_hMyWeapons))
        {
            if (handle is 0 or 0xFFFFFFFF)
                continue;

            var candidate = ResolveEntityFromHandle(memory, entityList, handle);
            if (candidate == nint.Zero)
                continue;

            var candidateId = ReadWeaponDefinitionIndex(memory, offsets, candidate);
            if (!GameOffsets.IsGrenadeWeapon(candidateId))
                continue;

            ReadGrenadeState(memory, offsets, candidate, out var candidatePinPulled, out var candidateThrowStrength);

            if (preferPinPulled && candidatePinPulled)
            {
                weapon = candidate;
                weaponId = candidateId;
                pinPulled = true;
                throwStrength = candidateThrowStrength;
                return true;
            }

            if (!preferPinPulled)
            {
                fallbackWeapon = candidate;
                fallbackWeaponId = candidateId;
                fallbackPinPulled = candidatePinPulled;
                fallbackThrowStrength = candidateThrowStrength;
            }
        }

        if (fallbackWeapon == nint.Zero)
            return false;

        weapon = fallbackWeapon;
        weaponId = fallbackWeaponId;
        pinPulled = fallbackPinPulled;
        throwStrength = fallbackThrowStrength;
        return true;
    }

    private static List<uint> CollectWeaponHandles(
        ProcessMemory memory,
        nint weaponServices,
        nint weaponsOffset)
    {
        var handles = new List<uint>();
        if (weaponServices == nint.Zero)
            return handles;

        var baseAddress = weaponServices + weaponsOffset;
        ReadOnlySpan<(int CountOffset, int DataOffset)> layouts =
        [
            (0, 8),
            (16, 8),
            (8, 0)
        ];

        foreach (var (countOffset, dataOffset) in layouts)
        {
            var count = memory.Read<int>(baseAddress + countOffset);
            if (count < 0)
                count = Math.Abs(count);

            if (count is <= 0 or > 64)
                continue;

            var dataPtr = memory.ReadPtr(baseAddress + dataOffset);
            if (dataPtr == nint.Zero)
                continue;

            handles.Clear();
            for (var i = 0; i < count; i++)
            {
                var handle = memory.Read<uint>(dataPtr + (nint)(i * 4));
                if (handle is not (0 or 0xFFFFFFFF))
                    handles.Add(handle);
            }

            if (handles.Count > 0)
                return handles;
        }

        return handles;
    }

    private static bool TryFindHeldGrenadeViaEntityScan(
        int maxEntityScanIndex,
        ProcessMemory memory,
        GameOffsets offsets,
        nint entityList,
        nint clientBase,
        out nint weapon,
        out ushort weaponId,
        out bool pinPulled,
        out float throwStrength,
        out string detectSource)
    {
        weapon = nint.Zero;
        weaponId = 0;
        pinPulled = false;
        throwStrength = 0f;
        detectSource = string.Empty;

        if (offsets.M_bIsHeldByPlayer == nint.Zero || offsets.DwGameEntitySystemHighestEntityIndex == nint.Zero)
            return false;

        var highestIndex = memory.Read<int>(clientBase + offsets.DwGameEntitySystemHighestEntityIndex);
        highestIndex = Math.Clamp(highestIndex, 0, maxEntityScanIndex);

        nint fallbackWeapon = nint.Zero;
        ushort fallbackWeaponId = 0;
        var fallbackPinPulled = false;
        var fallbackThrowStrength = 0f;

        for (var index = 64; index <= highestIndex; index++)
        {
            foreach (var spacing in GameOffsets.EntitySpacings)
            {
                var entity = ResolveEntityFromIndex(memory, entityList, index, spacing);
                if (entity == nint.Zero)
                    continue;

                if (memory.Read<byte>(entity + offsets.M_bIsHeldByPlayer) == 0)
                    continue;

                var candidateId = ReadWeaponDefinitionIndex(memory, offsets, entity);
                if (!GameOffsets.IsGrenadeWeapon(candidateId))
                    continue;

                ReadGrenadeState(memory, offsets, entity, out var candidatePinPulled, out var candidateThrowStrength);

                if (candidatePinPulled)
                {
                    weapon = entity;
                    weaponId = candidateId;
                    pinPulled = true;
                    throwStrength = candidateThrowStrength;
                    detectSource = "held-entity-pin";
                    return true;
                }

                fallbackWeapon = entity;
                fallbackWeaponId = candidateId;
                fallbackPinPulled = candidatePinPulled;
                fallbackThrowStrength = candidateThrowStrength;
            }
        }

        if (fallbackWeapon == nint.Zero)
            return false;

        weapon = fallbackWeapon;
        weaponId = fallbackWeaponId;
        pinPulled = fallbackPinPulled;
        throwStrength = fallbackThrowStrength;
        detectSource = "held-entity";
        return true;
    }

    private static ushort ReadWeaponDefinitionIndex(ProcessMemory memory, GameOffsets offsets, nint weapon) =>
        memory.Read<ushort>(weapon + offsets.M_AttributeManager + offsets.M_Item + offsets.M_iItemDefinitionIndex);

    private static void ReadGrenadeState(
        ProcessMemory memory,
        GameOffsets offsets,
        nint weapon,
        out bool pinPulled,
        out float throwStrength)
    {
        pinPulled = offsets.M_bPinPulled != nint.Zero
            && memory.Read<byte>(weapon + offsets.M_bPinPulled) != 0;

        throwStrength = offsets.M_flThrowStrength != nint.Zero
            ? memory.Read<float>(weapon + offsets.M_flThrowStrength)
            : 0f;
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
        IReadOnlyList<List<Vector3>> segments,
        IReadOnlyList<Vector3> bouncePoints,
        Vector3 landingPoint,
        int bounceCount) =>
        new()
        {
            IsActive = points.Count >= 2,
            Source = source,
            WeaponId = weaponId,
            Points = points,
            Segments = segments.Select(segment => (IReadOnlyList<Vector3>)segment).ToList(),
            BouncePoints = bouncePoints,
            LandingPoint = landingPoint.IsValid ? landingPoint : points[^1],
            BounceCount = bounceCount
        };

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
