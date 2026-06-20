# WeaponCatalog

## Purpose

Static catalog of configurable weapons for layered profile settings and the `/api/weapons` endpoint.

## Key API

| Member | Description |
|--------|-------------|
| `All` | `IReadOnlyList<WeaponDefinition>` (id, name, category) |
| `GetCategory(ushort weaponId)` | Resolve `WeaponCategory` |
| `GetName(ushort weaponId)` | Display name |
| `CategoryKey(WeaponCategory)` | String key for layered settings (`"Rifle"`, `"Smg"`, etc.) |

## Behavior

- Used by `SettingsResolver` for weapon-type layering.
- Used by `LocalPlayerReader` / `RadarReader` for weapon names.
- Exposed to the config UI via REST API.

## Dependencies

None (static data).

## Configuration

Weapon list is code-defined; extend `All` to add supported guns.
