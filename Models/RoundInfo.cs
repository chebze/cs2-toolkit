namespace Cs2Toolkit.Models;

public sealed class RoundInfo
{
    public int TotalRoundsPlayed { get; init; }
    public int RoundStartCount { get; init; }
    public int RoundEndCount { get; init; }
    public bool IsFreezePeriod { get; init; }
    public bool IsWarmupPeriod { get; init; }
    public int GamePhase { get; init; }
    public int RoundWinStatus { get; init; }
    public int RoundWinnerTeam { get; init; }

    public static RoundInfo Empty { get; } = new();
}
