using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;
using NumericVector3 = System.Numerics.Vector3;

namespace CS2Toolkit.Game.Memory;

internal sealed class BulletTracerReader
{
    private const float MaxTraceDistanceUnits = 8192f;
    private const int MaxBulletsPerDetection = 12;

    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly MapVisibilityChecker _mapChecker;
    private readonly Dictionary<int, int> _shotsFiredByPlayer = new();

    public BulletTracerReader(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker)
    {
        _memory = memory;
        _offsets = offsets;
        _mapChecker = mapChecker;
    }

    public IReadOnlyList<BulletImpactEvent> Detect(LegacyMemoryState state)
    {
        if (!_memory.IsAttached
            || !state.IsInMatch
            || _offsets.M_iShotsFired == nint.Zero
            || _offsets.M_angEyeAngles == nint.Zero)
        {
            _shotsFiredByPlayer.Clear();
            return [];
        }

        var clientBase = _memory.ClientBase;
        var entityList = _memory.ReadPtr(clientBase + _offsets.DwEntityList);
        if (entityList == nint.Zero)
        {
            _shotsFiredByPlayer.Clear();
            return [];
        }

        var events = new List<BulletImpactEvent>();
        var seen = new HashSet<int>();
        var timestamp = DateTimeOffset.UtcNow;

        foreach (var player in state.Players)
        {
            if (!player.IsAlive)
                continue;

            seen.Add(player.Index);

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var shotsFired = _memory.Read<int>(pawn + _offsets.M_iShotsFired);
            _shotsFiredByPlayer.TryGetValue(player.Index, out var previousShots);

            if (shotsFired > previousShots
                && TryReadEyePosition(pawn, out var eyePosition)
                && TryReadForwardDirection(pawn, out var forward))
            {
                var bulletCount = Math.Min(shotsFired - previousShots, MaxBulletsPerDetection);
                var endPosition = TraceBulletEnd(eyePosition, forward);
                var kind = Classify(player, state.LocalTeam);

                for (var i = 0; i < bulletCount; i++)
                {
                    events.Add(new BulletImpactEvent(
                        new PlayerId(player.Index),
                        kind,
                        eyePosition,
                        endPosition,
                        timestamp));
                }
            }

            _shotsFiredByPlayer[player.Index] = shotsFired;
        }

        foreach (var index in _shotsFiredByPlayer.Keys.Where(index => !seen.Contains(index)).ToList())
            _shotsFiredByPlayer.Remove(index);

        return events;
    }

    private static BulletTracerKind Classify(LegacyPlayerInfo player, int localTeam)
    {
        if (player.IsLocalPlayer)
            return BulletTracerKind.Local;

        return player.Team == localTeam
            ? BulletTracerKind.Teammate
            : BulletTracerKind.Enemy;
    }

    private Vector3 TraceBulletEnd(Vector3 eyePosition, Vector3 forward)
    {
        var direction = ToNumeric(forward);
        if (_mapChecker.TryRaycast(
                ToNumeric(eyePosition),
                direction,
                MaxTraceDistanceUnits,
                out var hitPoint,
                out _,
                out _))
        {
            return new Vector3(hitPoint.X, hitPoint.Y, hitPoint.Z);
        }

        return new Vector3(
            eyePosition.X + forward.X * MaxTraceDistanceUnits,
            eyePosition.Y + forward.Y * MaxTraceDistanceUnits,
            eyePosition.Z + forward.Z * MaxTraceDistanceUnits);
    }

    private bool TryReadEyePosition(nint pawn, out Vector3 eyePosition)
    {
        var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn);
        if (!position.IsValid)
        {
            eyePosition = default;
            return false;
        }

        eyePosition = new Vector3(position.X, position.Y, position.Z);
        if (_offsets.M_vecViewOffset == nint.Zero)
            return true;

        var viewOffset = ReadVector(pawn + _offsets.M_vecViewOffset);
        eyePosition = new Vector3(
            eyePosition.X + viewOffset.X,
            eyePosition.Y + viewOffset.Y,
            eyePosition.Z + viewOffset.Z);

        return true;
    }

    private bool TryReadForwardDirection(nint pawn, out Vector3 forward)
    {
        var pitch = _memory.Read<float>(pawn + _offsets.M_angEyeAngles);
        var yaw = _memory.Read<float>(pawn + _offsets.M_angEyeAngles + 4);

        var pitchRadians = DegreesToRadians(pitch);
        var yawRadians = DegreesToRadians(yaw);

        forward = new Vector3(
            MathF.Cos(pitchRadians) * MathF.Cos(yawRadians),
            MathF.Cos(pitchRadians) * MathF.Sin(yawRadians),
            -MathF.Sin(pitchRadians));

        var length = MathF.Sqrt(
            forward.X * forward.X + forward.Y * forward.Y + forward.Z * forward.Z);
        if (length <= 0.0001f)
            return false;

        forward = new Vector3(
            forward.X / length,
            forward.Y / length,
            forward.Z / length);

        return true;
    }

    private nint ResolvePawnForPlayer(nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var controller = _memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
            if (controller == nint.Zero)
                continue;

            var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
            if (pawnHandle is 0 or 0xFFFFFFFF)
                return nint.Zero;

            return ResolveEntityFromHandle(entityList, pawnHandle);
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((handle & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var entity = _memory.ReadPtr(listEntry + (nint)(spacing * (handle & 0x1FF)));
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }

    private Vector3 ReadVector(nint address) => new(
        _memory.Read<float>(address),
        _memory.Read<float>(address + 4),
        _memory.Read<float>(address + 8));

    private static float DegreesToRadians(float degrees) => degrees * (MathF.PI / 180f);

    private static NumericVector3 ToNumeric(Vector3 vector) => new(vector.X, vector.Y, vector.Z);
}
