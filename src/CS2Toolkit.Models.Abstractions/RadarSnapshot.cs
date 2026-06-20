namespace CS2Toolkit.Models.Abstractions;

public sealed record RadarPlayerSnapshot(
    int Id,
    string Name,
    int Team,
    int Health,
    bool IsLocalPlayer,
    float X,
    float Y,
    float Z,
    float Yaw,
    ushort ActiveWeaponId,
    string ActiveWeapon);

public sealed record RadarBombSnapshot(bool Planted, float X, float Y, float Z)
{
    public static RadarBombSnapshot Hidden { get; } = new(false, 0, 0, 0);
}

public sealed record RadarSnapshot(
    bool Attached,
    bool InMatch,
    string? Map,
    int LocalTeam,
    IReadOnlyList<RadarPlayerSnapshot> Players,
    RadarBombSnapshot Bomb,
    DateTimeOffset Timestamp)
{
    public static RadarSnapshot Idle { get; } = new(
        false, false, null, 0, [], RadarBombSnapshot.Hidden, DateTimeOffset.UtcNow);

    public static RadarSnapshot NotInMatch(bool attached, int localTeam = 0) => new(
        attached, false, null, localTeam, [], RadarBombSnapshot.Hidden, DateTimeOffset.UtcNow);
}
