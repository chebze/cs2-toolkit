using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;


namespace CS2Toolkit.Game.Memory;

internal static class BombSiteHelper
{
    public static LegacyBombSitesInfo TryReadSites(ProcessMemory memory, GameOffsets offsets, nint entityList)
    {
        for (var index = 0; index <= GameOffsets.MaxPlayerIndex; index++)
        {
            foreach (var spacing in GameOffsets.EntitySpacings)
            {
                var entity = ResolveEntityFromIndex(memory, entityList, index, spacing);
                if (entity == nint.Zero)
                    continue;

                var centerA = ReadVector(memory, entity + offsets.M_bombsiteCenterA);
                var centerB = ReadVector(memory, entity + offsets.M_bombsiteCenterB);
                if (!centerA.IsValid || !centerB.IsValid)
                    continue;

                if (offsets.M_foundGoalPositions != nint.Zero
                    && memory.Read<byte>(entity + offsets.M_foundGoalPositions) == 0)
                    continue;

                return new LegacyBombSitesInfo
                {
                    CenterA = centerA,
                    CenterB = centerB
                };
            }
        }

        return LegacyBombSitesInfo.Empty;
    }

    public static nint ResolvePlantedC4Entity(ProcessMemory memory, GameOffsets offsets, nint clientBase)
    {
        var first = memory.ReadPtr(clientBase + offsets.DwPlantedC4);
        if (first == nint.Zero)
            return nint.Zero;

        if (LooksLikePlantedC4(memory, offsets, first))
            return first;

        var second = memory.ReadPtr(first);
        if (second != nint.Zero && LooksLikePlantedC4(memory, offsets, second))
            return second;

        return first;
    }

    public static LegacyVector3 ReadEntityPosition(ProcessMemory memory, GameOffsets offsets, nint entity)
    {
        var sceneNodePosition = TryReadSceneNodeOrigin(memory, offsets, entity);
        if (sceneNodePosition.IsValid)
            return sceneNodePosition;

        if (offsets.M_vOldOrigin != nint.Zero)
        {
            var oldOrigin = ReadVector(memory, entity + offsets.M_vOldOrigin);
            if (oldOrigin.IsValid)
                return oldOrigin;
        }

        return default;
    }

    public static LegacyVector3? TryResolveSiteCenter(string? site, LegacyBombSitesInfo bombSites)
    {
        if (!bombSites.IsValid || string.IsNullOrEmpty(site))
            return null;

        return site switch
        {
            "A" => bombSites.CenterA,
            "B" => bombSites.CenterB,
            _ => null
        };
    }

    private static bool LooksLikePlantedC4(ProcessMemory memory, GameOffsets offsets, nint entity)
    {
        if (entity == nint.Zero)
            return false;

        var blowTime = memory.Read<float>(entity + offsets.M_flC4Blow);
        if (blowTime > 1f)
            return true;

        return TryReadSceneNodeOrigin(memory, offsets, entity).IsValid;
    }

    private static LegacyVector3 TryReadSceneNodeOrigin(ProcessMemory memory, GameOffsets offsets, nint entity)
    {
        var sceneNode = memory.ReadPtr(entity + offsets.M_pGameSceneNode);
        if (sceneNode == nint.Zero)
            return default;

        return ReadVector(memory, sceneNode + offsets.M_vecAbsOrigin);
    }

    private static LegacyVector3 ReadVector(ProcessMemory memory, nint address) =>
        new(
            memory.Read<float>(address),
            memory.Read<float>(address + 4),
            memory.Read<float>(address + 8));

    private static nint ResolveEntityFromIndex(ProcessMemory memory, nint entityList, int index, int spacing)
    {
        var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
    }
}
