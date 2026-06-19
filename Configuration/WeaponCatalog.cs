namespace Cs2Toolkit.Configuration;

public enum WeaponCategory
{
    Sniper,
    Smg,
    Pistol,
    Rifle,
    Shotgun,
    Other
}

public sealed record WeaponDefinition(ushort Id, string Name, WeaponCategory Category);

public static class WeaponCatalog
{
    public static readonly IReadOnlyList<WeaponDefinition> All =
    [
        new(9, "AWP", WeaponCategory.Sniper),
        new(40, "SSG 08", WeaponCategory.Sniper),
        new(11, "G3SG1", WeaponCategory.Sniper),
        new(38, "SCAR-20", WeaponCategory.Sniper),
        new(34, "MP9", WeaponCategory.Smg),
        new(17, "MAC-10", WeaponCategory.Smg),
        new(33, "MP7", WeaponCategory.Smg),
        new(24, "UMP-45", WeaponCategory.Smg),
        new(19, "P90", WeaponCategory.Smg),
        new(26, "PP-Bizon", WeaponCategory.Smg),
        new(23, "MP5-SD", WeaponCategory.Smg),
        new(4, "Glock-18", WeaponCategory.Pistol),
        new(61, "USP-S", WeaponCategory.Pistol),
        new(32, "P2000", WeaponCategory.Pistol),
        new(36, "P250", WeaponCategory.Pistol),
        new(3, "Five-SeveN", WeaponCategory.Pistol),
        new(30, "Tec-9", WeaponCategory.Pistol),
        new(63, "CZ75-Auto", WeaponCategory.Pistol),
        new(1, "Desert Eagle", WeaponCategory.Pistol),
        new(2, "Dual Berettas", WeaponCategory.Pistol),
        new(64, "R8 Revolver", WeaponCategory.Pistol),
        new(7, "AK-47", WeaponCategory.Rifle),
        new(16, "M4A4", WeaponCategory.Rifle),
        new(60, "M4A1-S", WeaponCategory.Rifle),
        new(13, "Galil AR", WeaponCategory.Rifle),
        new(10, "FAMAS", WeaponCategory.Rifle),
        new(39, "SG 553", WeaponCategory.Rifle),
        new(8, "AUG", WeaponCategory.Rifle),
        new(35, "Nova", WeaponCategory.Shotgun),
        new(25, "XM1014", WeaponCategory.Shotgun),
        new(27, "MAG-7", WeaponCategory.Shotgun),
        new(29, "Sawed-Off", WeaponCategory.Shotgun)
    ];

    private static readonly Dictionary<ushort, WeaponDefinition> ById =
        All.ToDictionary(w => w.Id);

    public static WeaponCategory GetCategory(ushort weaponId) =>
        ById.TryGetValue(weaponId, out var def) ? def.Category : WeaponCategory.Other;

    public static string GetName(ushort weaponId) =>
        ById.TryGetValue(weaponId, out var def) ? def.Name : $"Weapon {weaponId}";

    public static string CategoryKey(WeaponCategory category) => category switch
    {
        WeaponCategory.Sniper => "Sniper",
        WeaponCategory.Smg => "Smg",
        WeaponCategory.Pistol => "Pistol",
        WeaponCategory.Rifle => "Rifle",
        WeaponCategory.Shotgun => "Shotgun",
        _ => "Other"
    };
}
