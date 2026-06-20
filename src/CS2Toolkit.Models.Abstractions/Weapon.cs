namespace CS2Toolkit.Models.Abstractions;

public sealed record Weapon(
    WeaponId Id,
    string Name,
    WeaponType Type,
    WeaponCategory Category);
