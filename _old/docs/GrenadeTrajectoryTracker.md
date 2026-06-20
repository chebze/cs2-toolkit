# GrenadeTrajectoryTracker

## Purpose

Polls grenade trajectory state each memory tick and exposes the latest snapshot for `GrenadeOverlay`.

## Key API

| Member | Description |
|--------|-------------|
| `Snapshot` | Latest `GrenadeTrajectorySnapshot` |
| `LastStatus` | Resolver status string for diagnostics |
| `Initialize(offsets, mapChecker)` | One-time setup |
| `Poll(memory, state)` | Updates snapshot each tick |

## Behavior

- Delegates resolution to `GrenadeTrajectoryResolver`
- Clears snapshot when not in match or not ready
- Optional diagnostic logging via `Overlay:GrenadeTrajectory:LogDiagnostics`

## Dependencies

- `GrenadeTrajectoryResolver`
- `MapVisibilityChecker`
