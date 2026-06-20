# GrenadeTrajectoryResolver

## Purpose

Resolves active grenade trajectories from game memory and simulated throw previews.

## Key API

| Method | Description |
|--------|-------------|
| `Resolve(...)` | Active grenades in flight or recently thrown |
| `ResolveAimPreview(...)` | Simulated arc from local player throw state |

## Behavior

- Uses `GrenadeTrajectorySimulator` for physics when game trail is insufficient
- Raycasts against `MapVisibilityChecker` for bounces and landing
- Returns `GrenadeTrajectoryDiagnostics` with snapshot and status

## Configuration

`Toolkit:Grenade` — simulation parameters.

## Dependencies

- `GrenadeTrajectorySimulator`
- `BombSiteHelper` — entity positions
- `MapVisibilityChecker`
