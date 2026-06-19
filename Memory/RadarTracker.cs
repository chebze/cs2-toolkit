using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

public sealed class RadarTracker
{
    private GameOffsets? _offsets;

    public void Initialize(GameOffsets offsets) => _offsets = offsets;

    public RadarSnapshot BuildSnapshot(
        ProcessMemory memory,
        MemoryState state,
        string? mapName)
    {
        if (_offsets is null || !memory.IsAttached)
            return RadarSnapshot.Idle;

        if (!state.IsInMatch || string.IsNullOrWhiteSpace(mapName))
        {
            return RadarSnapshot.NotInMatch(attached: true, state.LocalTeam);
        }

        var clientBase = memory.ClientBase;
        var entityList = memory.ReadPtr(clientBase + _offsets.DwEntityList);
        if (entityList == nint.Zero)
            return RadarSnapshot.NotInMatch(attached: true, state.LocalTeam);

        var players = new List<RadarPlayerSnapshot>();
        foreach (var player in state.Players)
        {
            if (!player.IsAlive || player.Health < 1)
                continue;

            var pawn = ResolvePawnForPlayer(memory, entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var position = BombSiteHelper.ReadEntityPosition(memory, _offsets, pawn);
            if (!position.IsValid)
                continue;

            var weaponId = ReadActiveWeaponId(memory, entityList, pawn);
            players.Add(new RadarPlayerSnapshot
            {
                Id = player.Index,
                Name = player.Name,
                Team = player.Team,
                Health = player.Health,
                IsLocalPlayer = player.IsLocalPlayer,
                X = position.X,
                Y = position.Y,
                Z = position.Z,
                Yaw = ReadYaw(memory, pawn),
                ActiveWeaponId = weaponId,
                ActiveWeapon = WeaponDefinitionNames.GetName(weaponId)
            });
        }

        var bomb = BuildBombSnapshot(state.Bomb);

        return new RadarSnapshot
        {
            Attached = true,
            InMatch = true,
            Map = mapName,
            LocalTeam = state.LocalTeam,
            Players = players,
            Bomb = bomb,
            Timestamp = DateTime.UtcNow
        };
    }

    private static RadarBombSnapshot BuildBombSnapshot(BombInfo bomb)
    {
        if (bomb.Status is not (BombStatus.Planted or BombStatus.Defusing))
            return RadarBombSnapshot.Hidden;

        if (bomb.WorldPosition is not { IsValid: true } position)
            return new RadarBombSnapshot { Planted = true };

        return new RadarBombSnapshot
        {
            Planted = true,
            X = position.X,
            Y = position.Y,
            Z = position.Z
        };
    }

    private float ReadYaw(ProcessMemory memory, nint pawn)
    {
        if (_offsets!.M_angEyeAngles == nint.Zero)
            return 0f;

        return memory.Read<float>(pawn + _offsets.M_angEyeAngles + 4);
    }

    private ushort ReadActiveWeaponId(ProcessMemory memory, nint entityList, nint pawn)
    {
        var weaponServices = memory.ReadPtr(pawn + _offsets!.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return 0;

        var activeHandle = memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return 0;

        var weapon = ResolveEntityFromHandle(memory, entityList, activeHandle);
        if (weapon == nint.Zero)
            return 0;

        var itemAddr = memory.ReadPtr(weapon + _offsets.M_AttributeManager + _offsets.M_Item);
        if (itemAddr == nint.Zero)
            return 0;

        return memory.Read<ushort>(itemAddr + _offsets.M_iItemDefinitionIndex);
    }

    private nint ResolvePawnForPlayer(ProcessMemory memory, nint entityList, int index)
    {
        var controller = ResolveControllerFromIndex(memory, entityList, index);
        if (controller == nint.Zero)
            return nint.Zero;

        var pawnHandle = memory.Read<uint>(controller + _offsets!.M_hPlayerPawn);
        if (pawnHandle is 0 or 0xFFFFFFFF)
            return nint.Zero;

        return ResolvePawnFromHandle(memory, entityList, pawnHandle);
    }

    private nint ResolveControllerFromIndex(ProcessMemory memory, nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var controller = memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
            if (controller != nint.Zero)
                return controller;
        }

        return nint.Zero;
    }

    private nint ResolvePawnFromHandle(ProcessMemory memory, nint entityList, uint pawnHandle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var pawn = memory.ReadPtr(listEntry + (nint)(spacing * (pawnHandle & 0x1FF)));
            if (pawn != nint.Zero)
                return pawn;
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((handle & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var entity = memory.ReadPtr(listEntry + (nint)(spacing * (handle & 0x1FF)));
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }
}
