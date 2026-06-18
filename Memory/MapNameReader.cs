using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;
using System.Numerics;

namespace Cs2Toolkit.Memory;

public sealed class MapNameReader
{
    public string? ReadCurrentMap(ProcessMemory memory, GameOffsets offsets)
    {
        var matchmakingBase = memory.GetModuleBase("matchmaking.dll");
        if (matchmakingBase == nint.Zero || offsets.DwGameTypes == nint.Zero)
            return null;

        var mapNameAddress = memory.ReadPtr(matchmakingBase + offsets.DwGameTypes + GameOffsets.DwGameTypes_mapName);
        if (mapNameAddress == nint.Zero)
            return null;

        var mapName = memory.ReadString(mapNameAddress, 128);
        return string.IsNullOrWhiteSpace(mapName)
            ? null
            : MapVisibilityChecker.NormalizeMapName(mapName);
    }
}
