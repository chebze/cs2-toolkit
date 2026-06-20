# RadarReader

## Purpose

Builds a live `RadarSnapshot` from process memory each tick: player positions, yaw, health, names, active weapons, and planted bomb state.

## Key API

| Member | Description |
|--------|-------------|
| `Read(state, mapName)` | Returns radar data for the current match |

## Behavior

- Returns `RadarSnapshot.Idle` when not attached
- Returns `RadarSnapshot.NotInMatch` when attached but not in a live match or map name is missing
- Reads world position via `BombSiteHelper.ReadEntityPosition`
- Reads yaw from `M_angEyeAngles`
- Resolves active weapon via weapon services and item definition index
- Normalizes map name via `MapNameNormalizer`

## Dependencies

- `ProcessMemory`, `GameOffsets`, `LegacyMemoryState`, `WeaponCatalog`

## Configuration

None.
