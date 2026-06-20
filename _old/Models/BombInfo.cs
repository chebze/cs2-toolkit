namespace Cs2Toolkit.Models;

public sealed class BombInfo
{
    public BombStatus Status { get; init; }
    public string? Site { get; init; }
    public int? TimeLeftSeconds { get; init; }
    public bool? HasDefuseKit { get; init; }
    public int? DefuseTimeSeconds { get; init; }
    public bool? WillDefuseSucceed { get; init; }
    public Vector3? WorldPosition { get; init; }

    public bool IsVisible => Status != BombStatus.None;

    public static BombInfo Hidden { get; } = new() { Status = BombStatus.None };
}
