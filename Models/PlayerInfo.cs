namespace Cs2Toolkit.Models;

public sealed class PlayerInfo
{
    public int Index { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Team { get; init; }
    public int Health { get; init; }
    public bool IsAlive { get; init; }
    public bool IsLocalPlayer { get; init; }
}
