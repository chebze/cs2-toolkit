# RadarTracker

## Purpose

Builds a live radar snapshot from game memory each tick: player positions, yaw, health, names, active weapons, and bomb state.

## Key API

- `Initialize(GameOffsets offsets)`
- `BuildSnapshot(ProcessMemory memory, MemoryState state, string? mapName)` → `RadarSnapshot`

## Behavior

- Returns `RadarSnapshot.Idle` when not attached
- Returns `RadarSnapshot.NotInMatch` when attached but not in a live match
- Reads world position via `BombSiteHelper.ReadEntityPosition`
- Reads yaw from `M_angEyeAngles`
- Reads active weapon via weapon services + item definition index
- Maps weapon IDs to names via `WeaponDefinitionNames`

## Dependencies

- `ProcessMemory`, `GameOffsets`, `MemoryState`, `BombInfo`

## Configuration

None.
