namespace CS2Toolkit.Game.Internal;

using CS2Toolkit.Models.Abstractions;

internal sealed class LegacyMemoryState
{
    public bool IsAttached { get; init; }
    public bool IsInGame { get; init; }
    public bool IsInMatch { get; init; }
    public int LocalTeam { get; init; }
    public IReadOnlyList<LegacyPlayerInfo> Players { get; init; } = Array.Empty<LegacyPlayerInfo>();
    public int EnemiesAlive { get; init; }
    public int EnemiesDead { get; init; }
    public int TeammatesAlive { get; init; }
    public int TeammatesDead { get; init; }
    public LegacyRoundInfo Round { get; init; } = LegacyRoundInfo.Empty;
    public LegacyBombInfo Bomb { get; init; } = LegacyBombInfo.Hidden;
    public LegacyBombSitesInfo BombSites { get; init; } = LegacyBombSitesInfo.Empty;
    public IReadOnlyList<string> ClairvoyanceTips { get; init; } = Array.Empty<string>();

    public static LegacyMemoryState Detached { get; } = new();
}

internal sealed class LegacyPlayerInfo
{
    public int Index { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Team { get; init; }
    public int Health { get; init; }
    public bool IsAlive { get; init; }
    public bool IsLocalPlayer { get; init; }
    public Vector3? WorldPosition { get; init; }
    public PlayerBones? Bones { get; set; }
    public bool IsSpottedByTeam { get; set; }
    public bool IsVisibleToLocalPlayer { get; set; }
}

internal sealed class LegacyRoundInfo
{
    public int TotalRoundsPlayed { get; init; }
    public int RoundStartCount { get; init; }
    public int RoundEndCount { get; init; }
    public bool IsFreezePeriod { get; init; }
    public bool IsWarmupPeriod { get; init; }
    public int GamePhase { get; init; }
    public int RoundWinStatus { get; init; }
    public int RoundWinnerTeam { get; init; }

    public static LegacyRoundInfo Empty { get; } = new();
}

internal enum LegacyBombStatus
{
    None,
    Carried,
    Equipped,
    OnGround,
    Defusing,
    Planting,
    Planted
}

internal sealed class LegacyBombInfo
{
    public LegacyBombStatus Status { get; init; }
    public string? Site { get; init; }
    public int? TimeLeftSeconds { get; init; }
    public bool? HasDefuseKit { get; init; }
    public int? DefuseTimeSeconds { get; init; }
    public bool? WillDefuseSucceed { get; init; }
    public LegacyVector3? WorldPosition { get; init; }

    public static LegacyBombInfo Hidden { get; } = new() { Status = LegacyBombStatus.None };
}

internal readonly struct LegacyVector3(float x, float y, float z)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Z { get; } = z;

    public bool IsValid =>
        !float.IsNaN(X) && !float.IsNaN(Y) && !float.IsNaN(Z)
        && (MathF.Abs(X) > 1f || MathF.Abs(Y) > 1f || MathF.Abs(Z) > 1f);

    public float DistanceTo2D(LegacyVector3 other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

internal sealed class LegacyBombSitesInfo
{
    public LegacyVector3 CenterA { get; init; }
    public LegacyVector3 CenterB { get; init; }

    public bool IsValid => CenterA.IsValid && CenterB.IsValid;

    public static LegacyBombSitesInfo Empty { get; } = new();

    public string? LabelForPosition(LegacyVector3 position)
    {
        if (!IsValid || !position.IsValid)
            return null;

        return position.DistanceTo2D(CenterA) <= position.DistanceTo2D(CenterB) ? "A" : "B";
    }
}
