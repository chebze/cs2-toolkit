namespace CS2Toolkit.Models.Abstractions;

public sealed record LocalPlayer(
    PlayerId Id,
    Team Team,
    int Health,
    WeaponId ActiveWeaponId,
    string ActiveWeaponName,
    WeaponType ActiveWeaponType,
    Vector3 Position,
    Vector3 ViewAngles);
