using CS2Toolkit.Configuration;
using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Readers;

internal sealed class ViewMatrixReader
{
    public ViewMatrix Read(ProcessMemory memory, GameOffsets offsets)
    {
        Span<float> values = stackalloc float[ViewMatrix.FloatCount];
        if (!memory.IsAttached)
            return new ViewMatrix(values);

        var matrixAddress = memory.ClientBase + offsets.DwViewMatrix;
        for (var i = 0; i < ViewMatrix.FloatCount; i++)
            values[i] = memory.Read<float>(matrixAddress + (nint)(i * 4));

        return new ViewMatrix(values);
    }
}

internal sealed class LocalPlayerReader
{
    public LocalPlayer? Read(ProcessMemory memory, GameOffsets offsets, LegacyMemoryState state)
    {
        if (!memory.IsAttached || !state.IsInGame)
            return null;

        var clientBase = memory.ClientBase;
        var entityList = memory.ReadPtr(clientBase + offsets.DwEntityList);
        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        if (entityList == nint.Zero || localPawn == nint.Zero)
            return null;

        var local = state.Players.FirstOrDefault(p => p.IsLocalPlayer);
        var team = local is null ? Team.None : MapTeam(local.Team);
        var health = local?.Health ?? memory.Read<int>(localPawn + offsets.M_iHealth);
        var weaponId = ReadActiveWeaponId(memory, offsets, entityList, localPawn);
        var name = WeaponCatalog.GetName(weaponId);
        var category = WeaponCatalog.GetCategory(weaponId);

        var origin = memory.Read<float>(localPawn + offsets.M_vOldOrigin);
        var originY = memory.Read<float>(localPawn + offsets.M_vOldOrigin + 4);
        var originZ = memory.Read<float>(localPawn + offsets.M_vOldOrigin + 8);
        var pitch = memory.Read<float>(localPawn + offsets.M_angEyeAngles);
        var yaw = memory.Read<float>(localPawn + offsets.M_angEyeAngles + 4);

        return new LocalPlayer(
            new PlayerId(local?.Index ?? 0),
            team,
            health,
            new WeaponId(weaponId),
            name,
            MapWeaponType(category),
            new Vector3(origin, originY, originZ),
            new Vector3(pitch, yaw, 0));
    }

    private static ushort ReadActiveWeaponId(ProcessMemory memory, GameOffsets offsets, nint entityList, nint localPawn)
    {
        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return 0;

        var activeHandle = memory.Read<uint>(weaponServices + offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return 0;

        var weapon = ResolveEntityFromHandle(memory, entityList, activeHandle);
        if (weapon == nint.Zero)
            return 0;

        return memory.Read<ushort>(
            weapon + offsets.M_AttributeManager + offsets.M_Item + offsets.M_iItemDefinitionIndex);
    }

    private static nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle)
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

    private static Team MapTeam(int team) => team switch
    {
        GameOffsets.TeamTerrorist => Team.Terrorist,
        GameOffsets.TeamCounterTerrorist => Team.CounterTerrorist,
        _ => Team.None
    };

    private static WeaponType MapWeaponType(WeaponCategory category) => category switch
    {
        WeaponCategory.Sniper => WeaponType.Sniper,
        WeaponCategory.Smg => WeaponType.Smg,
        WeaponCategory.Pistol => WeaponType.Pistol,
        WeaponCategory.Rifle => WeaponType.Rifle,
        WeaponCategory.Shotgun => WeaponType.Shotgun,
        _ => WeaponType.Other
    };
}
