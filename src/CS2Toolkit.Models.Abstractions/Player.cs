namespace CS2Toolkit.Models.Abstractions;

public sealed record Player(
    PlayerId Id,
    string Name,
    Team Team,
    int Health,
    bool IsAlive,
    bool IsLocalPlayer);
