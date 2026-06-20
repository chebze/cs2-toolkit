namespace CS2Toolkit.Models.Abstractions;

public sealed record BombState(
    BombStatus Status,
    string? Site,
    int? TimeLeftSeconds,
    bool? HasDefuseKit,
    int? DefuseTimeSeconds,
    bool? WillDefuseSucceed,
    Vector3? WorldPosition)
{
    public bool IsVisible => Status != BombStatus.None;

    public static BombState Hidden { get; } = new(BombStatus.None, null, null, null, null, null, null);
}
