# EnemyEspTracker

## Purpose

Maintains drawable enemy ESP targets from mapped `GameSnapshot` data. Ports legacy `EnemyLastSeenTracker` without reading process memory.

## Key API

| Member | Description |
|--------|-------------|
| `Update(snapshot, mode)` | Refreshes internal last-seen or live target caches |
| `CopyDrawableTargets(mode)` | Returns thread-safe snapshot of targets to draw |

## Behavior

- **LastSeen:** captures enemy skeletons when spotted by the local team but not visible to the local player; hides targets currently visible to the local player
- **Full:** tracks live skeletons for all alive enemies each tick
- Clears caches on round start, detach, or when mode is `Disabled`
- Thread-safe via internal lock; overlay reads via `CopyDrawableTargets`

## Dependencies

- `GameSnapshot`, `Player`, `EspTarget`, `EnemyEspMode`

## Configuration

Runtime mode from `IFeatureState.EnemyEspMode` (keybind cycle: Disabled → LastSeen → Full).
