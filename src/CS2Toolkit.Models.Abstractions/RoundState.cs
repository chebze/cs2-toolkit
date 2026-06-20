namespace CS2Toolkit.Models.Abstractions;

public sealed record RoundState(
    int TotalRoundsPlayed,
    int RoundStartCount,
    int RoundEndCount,
    bool IsFreezePeriod,
    bool IsWarmupPeriod,
    int GamePhase,
    int RoundWinStatus,
    int RoundWinnerTeam)
{
    public static RoundState Empty { get; } = new(0, 0, 0, false, false, 0, 0, 0);
}
