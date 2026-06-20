using CS2Toolkit.Configuration;
using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Game.Readers;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Memory;

internal sealed class RadarReader
{
    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;

    public RadarReader(ProcessMemory memory, GameOffsets offsets)
    {
        _memory = memory;
        _offsets = offsets;
    }

    public RadarSnapshot Read(LegacyMemoryState state, string? mapName)
    {
        if (!_memory.IsAttached)
            return RadarSnapshot.Idle;

        if (!state.IsInMatch || string.IsNullOrWhiteSpace(mapName))
            return RadarSnapshot.NotInMatch(attached: true, state.LocalTeam);

        var clientBase = _memory.ClientBase;
        var entityList = _memory.ReadPtr(clientBase + _offsets.DwEntityList);
        if (entityList == nint.Zero)
            return RadarSnapshot.NotInMatch(attached: true, state.LocalTeam);

        var players = new List<RadarPlayerSnapshot>();
        foreach (var player in state.Players)
        {
            if (!player.IsAlive || player.Health < 1)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn);
            if (!position.IsValid)
                continue;

            var weaponId = ReadActiveWeaponId(entityList, pawn);
            players.Add(new RadarPlayerSnapshot(
                player.Index,
                player.Name,
                player.Team,
                player.Health,
                player.IsLocalPlayer,
                position.X,
                position.Y,
                position.Z,
                ReadYaw(pawn),
                weaponId,
                WeaponCatalog.GetName(weaponId)));
        }

        return new RadarSnapshot(
            Attached: true,
            InMatch: true,
            Map: MapNameNormalizer.NormalizeMapName(mapName),
            LocalTeam: state.LocalTeam,
            Players: players,
            Bomb: BuildBombSnapshot(state.Bomb),
            Timestamp: DateTimeOffset.UtcNow);
    }

    private static RadarBombSnapshot BuildBombSnapshot(LegacyBombInfo bomb)
    {
        if (bomb.Status is not (LegacyBombStatus.Planted or LegacyBombStatus.Defusing))
            return RadarBombSnapshot.Hidden;

        if (bomb.WorldPosition is not { IsValid: true } position)
            return new RadarBombSnapshot(true, 0, 0, 0);

        return new RadarBombSnapshot(true, position.X, position.Y, position.Z);
    }

    private float ReadYaw(nint pawn)
    {
        if (_offsets.M_angEyeAngles == nint.Zero)
            return 0f;

        return _memory.Read<float>(pawn + _offsets.M_angEyeAngles + 4);
    }

    private ushort ReadActiveWeaponId(nint entityList, nint pawn)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return 0;

        var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return 0;

        var weapon = ResolveEntityFromHandle(entityList, activeHandle);
        if (weapon == nint.Zero)
            return 0;

        var itemAddr = _memory.ReadPtr(weapon + _offsets.M_AttributeManager + _offsets.M_Item);
        if (itemAddr == nint.Zero)
            return 0;

        return _memory.Read<ushort>(itemAddr + _offsets.M_iItemDefinitionIndex);
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

    private nint ResolvePawnFromHandle(nint entityList, uint pawnHandle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var pawn = _memory.ReadPtr(listEntry + (nint)(spacing * (pawnHandle & 0x1FF)));
            if (pawn != nint.Zero)
                return pawn;
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
}
