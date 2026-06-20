using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;



namespace CS2Toolkit.Game.Readers;

internal sealed class MapNameReader
{
    public string? ReadCurrentMap(ProcessMemory memory, GameOffsets offsets)
    {
        foreach (var candidate in ReadCandidates(memory, offsets))
        {
            var normalized = MapNameNormalizer.NormalizeMapName(candidate);
            if (!string.IsNullOrWhiteSpace(normalized))
                return normalized;
        }

        return null;
    }

    private static IEnumerable<string> ReadCandidates(ProcessMemory memory, GameOffsets offsets)
    {
        var matchmaking = ReadMatchmakingMapName(memory, offsets);
        if (!string.IsNullOrWhiteSpace(matchmaking))
            yield return matchmaking;

        var fromGlobalVars = ReadGlobalVarsMapName(memory, offsets, GameOffsets.GlobalVarsCurrentMapName);
        if (!string.IsNullOrWhiteSpace(fromGlobalVars))
            yield return fromGlobalVars;

        var fromGlobalVarsAlt = ReadGlobalVarsMapName(memory, offsets, GameOffsets.GlobalVarsCurrentMapNameAlt);
        if (!string.IsNullOrWhiteSpace(fromGlobalVarsAlt))
            yield return fromGlobalVarsAlt;
    }

    private static string ReadMatchmakingMapName(ProcessMemory memory, GameOffsets offsets)
    {
        var matchmakingBase = memory.GetModuleBase("matchmaking.dll");
        if (matchmakingBase == nint.Zero || offsets.DwGameTypes == nint.Zero)
            return string.Empty;

        var cutlStringAddress = matchmakingBase + offsets.DwGameTypes + GameOffsets.DwGameTypes_mapName;
        return ReadCuString(memory, cutlStringAddress);
    }

    private static string ReadGlobalVarsMapName(ProcessMemory memory, GameOffsets offsets, nint fieldOffset)
    {
        if (offsets.DwGlobalVars == nint.Zero)
            return string.Empty;

        var globalVars = memory.ReadPtr(memory.ClientBase + offsets.DwGlobalVars);
        if (globalVars == nint.Zero)
            return string.Empty;

        return ReadCuString(memory, globalVars + fieldOffset);
    }

    private static string ReadCuString(ProcessMemory memory, nint cutlStringAddress)
    {
        if (cutlStringAddress == nint.Zero)
            return string.Empty;

        var stringPtr = memory.ReadPtr(cutlStringAddress);
        if (stringPtr != nint.Zero)
        {
            var value = memory.ReadString(stringPtr, 128);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return memory.ReadString(cutlStringAddress + IntPtr.Size, 120);
    }
}
