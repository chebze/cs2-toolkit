using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Memory;

public sealed class EnemySoundTracker
{
    private readonly ProcessMemory _memory;
    private readonly EnemyNoiseOptions _options;
    private readonly Dictionary<int, EnemySoundSnapshot> _snapshots = new();

    private GameOffsets? _offsets;

    public EnemySoundTracker(ProcessMemory memory, IOptions<ToolkitOptions> options)
    {
        _memory = memory;
        _options = options.Value.EnemyNoise;
    }

    public event EventHandler<EnemyNoiseEventArgs>? OnEnemyNoise;

    public void Initialize(GameOffsets offsets) => _offsets = offsets;

    public void Poll(MemoryState state)
    {
        if (_offsets is null || !_memory.IsAttached || !state.IsInMatch)
        {
            _snapshots.Clear();
            return;
        }

        var clientBase = _memory.ClientBase;

        var entityList = _memory.ReadPtr(clientBase + _offsets.DwEntityList);
        var localPawn = _memory.ReadPtr(clientBase + _offsets.DwLocalPlayerPawn);

        if (entityList == nint.Zero || localPawn == nint.Zero)
            return;
        var localPosition = BombSiteHelper.ReadEntityPosition(_memory, _offsets, localPawn);
        if (!localPosition.IsValid)
            return;

        var seen = new HashSet<int>();

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == state.LocalTeam)
                continue;

            seen.Add(player.Index);

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn);
            if (!position.IsValid)
                continue;

            if (localPosition.DistanceTo(position) > _options.MaxDistanceUnits)
                continue;

            var snapshot = ReadSnapshot(pawn, entityList);
            if (!_snapshots.TryGetValue(player.Index, out var previous))
            {
                _snapshots[player.Index] = snapshot;
                continue;
            }

            if (TryDetectNoise(previous, snapshot, out var soundType))
            {
                OnEnemyNoise?.Invoke(this, new EnemyNoiseEventArgs
                {
                    PlayerIndex = player.Index,
                    SoundType = soundType,
                    WorldPosition = position
                });
            }

            _snapshots[player.Index] = snapshot;
        }

        foreach (var index in _snapshots.Keys.Where(index => !seen.Contains(index)).ToList())
            _snapshots.Remove(index);
    }

    private bool TryDetectNoise(
        EnemySoundSnapshot previous,
        EnemySoundSnapshot current,
        out EnemySoundType soundType)
    {
        soundType = EnemySoundType.Other;

        var emitChanged = current.EmitSoundTime > previous.EmitSoundTime + 0.001f;
        var jumpChanged = current.LastJumpTick != previous.LastJumpTick && current.LastJumpTick > 0;

        if (!emitChanged && !jumpChanged)
            return false;

        if (current.IsReloading)
            soundType = EnemySoundType.Reload;
        else if (jumpChanged)
            soundType = EnemySoundType.Jump;
        else if (current.IsWalking)
            soundType = EnemySoundType.Step;
        else
            soundType = EnemySoundType.Other;

        return true;
    }

    private EnemySoundSnapshot ReadSnapshot(nint pawn, nint entityList)
    {
        var offsets = _offsets!;
        var movement = offsets.M_pMovementServices != nint.Zero
            ? _memory.ReadPtr(pawn + offsets.M_pMovementServices)
            : nint.Zero;
        var lastJumpTick = movement != nint.Zero && offsets.M_nLastJumpTick != nint.Zero
            ? _memory.Read<int>(movement + offsets.M_nLastJumpTick)
            : 0;

        return new EnemySoundSnapshot
        {
            EmitSoundTime = _memory.Read<float>(pawn + offsets.M_flEmitSoundTime),
            IsWalking = _memory.Read<byte>(pawn + offsets.M_bIsWalking) != 0,
            IsReloading = IsReloading(pawn, entityList),
            LastJumpTick = lastJumpTick
        };
    }

    private bool IsReloading(nint pawn, nint entityList)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets!.M_pWeaponServices);
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

        var pawnHandle = _memory.Read<uint>(controller + _offsets!.M_hPlayerPawn);
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

    private readonly struct EnemySoundSnapshot
    {
        public float EmitSoundTime { get; init; }
        public bool IsWalking { get; init; }
        public bool IsReloading { get; init; }
        public int LastJumpTick { get; init; }
    }
}
