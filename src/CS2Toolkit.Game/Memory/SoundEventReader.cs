using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Memory;

internal sealed class SoundEventReader
{
    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly Dictionary<int, PlayerSoundSnapshot> _snapshots = new();

    public SoundEventReader(ProcessMemory memory, GameOffsets offsets)
    {
        _memory = memory;
        _offsets = offsets;
    }

    public IReadOnlyList<SoundEvent> Detect(
        nint entityList,
        nint localPawn,
        int localTeam,
        IReadOnlyList<LegacyPlayerInfo> players,
        bool isInMatch)
    {
        if (!isInMatch || entityList == nint.Zero || localPawn == nint.Zero
            || localTeam is not (GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist))
        {
            _snapshots.Clear();
            return [];
        }

        var localPosition = BombSiteHelper.ReadEntityPosition(_memory, _offsets, localPawn);
        if (!localPosition.IsValid)
            return [];

        var events = new List<SoundEvent>();
        var seen = new HashSet<int>();
        var timestamp = DateTimeOffset.UtcNow;

        foreach (var player in players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == localTeam)
                continue;

            seen.Add(player.Index);

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn);
            if (!position.IsValid)
                continue;

            var snapshot = ReadSnapshot(pawn, entityList);
            if (!_snapshots.TryGetValue(player.Index, out var previous))
            {
                _snapshots[player.Index] = snapshot;
                continue;
            }

            if (TryDetectNoise(previous, snapshot, out var kind))
            {
                events.Add(new SoundEvent(
                    new PlayerId(player.Index),
                    kind,
                    MapVector(position),
                    timestamp));
            }

            _snapshots[player.Index] = snapshot;
        }

        foreach (var index in _snapshots.Keys.Where(index => !seen.Contains(index)).ToList())
            _snapshots.Remove(index);

        return events;
    }

    private static bool TryDetectNoise(
        PlayerSoundSnapshot previous,
        PlayerSoundSnapshot current,
        out SoundKind kind)
    {
        kind = SoundKind.Other;

        var emitChanged = current.EmitSoundTime > previous.EmitSoundTime + 0.001f;
        var jumpChanged = current.LastJumpTick != previous.LastJumpTick && current.LastJumpTick > 0;

        if (!emitChanged && !jumpChanged)
            return false;

        if (current.IsReloading)
            kind = SoundKind.Reload;
        else if (jumpChanged)
            kind = SoundKind.Jump;
        else if (current.IsWalking)
            kind = SoundKind.Step;
        else
            kind = SoundKind.Other;

        return true;
    }

    private PlayerSoundSnapshot ReadSnapshot(nint pawn, nint entityList)
    {
        var movement = _offsets.M_pMovementServices != nint.Zero
            ? _memory.ReadPtr(pawn + _offsets.M_pMovementServices)
            : nint.Zero;
        var lastJumpTick = movement != nint.Zero && _offsets.M_nLastJumpTick != nint.Zero
            ? _memory.Read<int>(movement + _offsets.M_nLastJumpTick)
            : 0;

        return new PlayerSoundSnapshot
        {
            EmitSoundTime = _memory.Read<float>(pawn + _offsets.M_flEmitSoundTime),
            IsWalking = _memory.Read<byte>(pawn + _offsets.M_bIsWalking) != 0,
            IsReloading = IsReloading(pawn, entityList),
            LastJumpTick = lastJumpTick
        };
    }

    private bool IsReloading(nint pawn, nint entityList)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        var weapon = ResolveEntityFromHandle(entityList, activeHandle);
        if (weapon == nint.Zero || _offsets.M_bInReload == nint.Zero)
            return false;

        return _memory.Read<byte>(weapon + _offsets.M_bInReload) != 0;
    }

    private nint ResolvePawnForPlayer(nint entityList, int index)
    {
        var controller = ResolveControllerFromIndex(entityList, index);
        if (controller == nint.Zero)
            return nint.Zero;

        var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
        if (pawnHandle is 0 or 0xFFFFFFFF)
            return nint.Zero;

        return ResolvePawnFromHandle(entityList, pawnHandle);
    }

    private nint ResolveControllerFromIndex(nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var controller = _memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
            if (controller != nint.Zero)
                return controller;
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolvePawnFromHandle(entityList, handle, spacing);
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }

    private nint ResolvePawnFromHandle(nint entityList, uint pawnHandle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var pawn = ResolvePawnFromHandle(entityList, pawnHandle, spacing);
            if (pawn != nint.Zero)
                return pawn;
        }

        return nint.Zero;
    }

    private nint ResolvePawnFromHandle(nint entityList, uint pawnHandle, int spacing)
    {
        var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return _memory.ReadPtr(listEntry + (nint)(spacing * (pawnHandle & 0x1FF)));
    }

    private static Vector3 MapVector(LegacyVector3 vector) => new(vector.X, vector.Y, vector.Z);

    private readonly struct PlayerSoundSnapshot
    {
        public float EmitSoundTime { get; init; }
        public bool IsWalking { get; init; }
        public bool IsReloading { get; init; }
        public int LastJumpTick { get; init; }
    }
}
