# GrenadeTrajectoryResolver

## Purpose

Resolves the local player's active grenade throw trajectory from process memory and map raycasts.

## Key API

| Member | Description |
|--------|-------------|
| `Resolve(memory, offsets, mapChecker, clientBase, isInMatch)` | Returns diagnostics with an active trajectory snapshot when pin is pulled |
| `ResolveAimPreview(...)` | Preview trajectory for a specific grenade weapon id |
| `IsGrenadeAimLikely(...)` | Whether the active weapon is a grenade |

## Behavior

- Detects active grenade with pin pulled from weapon services
- Reads throw angles and simulates arc via `GrenadeTrajectorySimulator` and `MapVisibilityChecker`
- May fall back to game trail or stashed throw parameters when available
- Returns inactive snapshot when not in match or simulation fails

## Dependencies

- `ProcessMemory`, `GameOffsets`, `MapVisibilityChecker`, `GrenadeTrajectorySimulator`
- `GrenadeSimulationOptions`, `BombSiteHelper`

## Configuration

`GrenadeSimulationOptions` from `ToolkitHostSettings.Grenade` via `GrenadeSimulationOptionsFactory`.
