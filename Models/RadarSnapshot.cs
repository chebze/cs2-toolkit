namespace Cs2Toolkit.Models;

public sealed class RadarSnapshot
{
    public bool Attached { get; init; }
    public bool InMatch { get; init; }
    public string? Map { get; init; }
    public int LocalTeam { get; init; }
    public IReadOnlyList<RadarPlayerSnapshot> Players { get; init; } = [];
    public RadarBombSnapshot Bomb { get; init; } = RadarBombSnapshot.Hidden;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static RadarSnapshot Idle { get; } = new()
    {
        Attached = false,
        InMatch = false,
        Map = null,
        Players = [],
        Bomb = RadarBombSnapshot.Hidden
    };

    public static RadarSnapshot NotInMatch(bool attached, int localTeam = 0) => new()
    {
        Attached = attached,
        InMatch = false,
        Map = null,
        LocalTeam = localTeam,
        Players = [],
        Bomb = RadarBombSnapshot.Hidden
    };
}

public sealed class RadarPlayerSnapshot
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public int Team { get; init; }
    public int Health { get; init; }
    public bool IsLocalPlayer { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public float Yaw { get; init; }
    public ushort ActiveWeaponId { get; init; }
    public string ActiveWeapon { get; init; } = "UNKNOWN";
}

public sealed class RadarBombSnapshot
{
    public bool Planted { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }

    public static RadarBombSnapshot Hidden { get; } = new();
}
