namespace CS2Toolkit.Game.Internal;

internal sealed class ClairvoyanceOptionsStub;

internal sealed class ClairvoyanceAdvisorStub
{
    public ClairvoyanceAdvisorStub(
        Process.ProcessMemory memory,
        GameOffsets offsets,
        ClairvoyanceOptionsStub options)
    {
    }

    public IReadOnlyList<string> ResolveTips(
        nint clientBase,
        nint entityList,
        nint localPawn,
        int localTeam,
        IReadOnlyList<LegacyPlayerInfo> players,
        LegacyBombInfo bomb,
        LegacyBombSitesInfo bombSites) => [];
}
