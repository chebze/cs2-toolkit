namespace Cs2Toolkit.Models;

public sealed class MemoryState
{
    public bool IsAttached { get; init; }
    public bool IsInGame { get; init; }
    public bool IsInMatch { get; init; }
    public int LocalTeam { get; init; }
    public IReadOnlyList<PlayerInfo> Players { get; init; } = Array.Empty<PlayerInfo>();

    public int EnemiesAlive { get; init; }
    public int EnemiesDead { get; init; }
    public int TeammatesAlive { get; init; }
    public int TeammatesDead { get; init; }
    public RoundInfo Round { get; init; } = RoundInfo.Empty;
    public BombInfo Bomb { get; init; } = BombInfo.Hidden;
    public BombSitesInfo BombSites { get; init; } = BombSitesInfo.Empty;
    public IReadOnlyList<string> ClairvoyanceTips { get; init; } = Array.Empty<string>();

    public static MemoryState Detached { get; } = new();
}
