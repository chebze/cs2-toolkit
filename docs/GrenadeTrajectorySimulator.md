# GrenadeTrajectorySimulator

## Purpose

Physics simulator for CS2 grenade throws with map collision raycasts.

## Key API

| Member | Description |
|--------|-------------|
| `TrySimulate(mapChecker, start, velocity, ...)` | Produces arc points, segments, bounce points, and landing |
| `TryComputeThrowState(...)` | Computes throw start and velocity from eye position and angles |

## Behavior

- Tick-based simulation with gravity, bounce elasticity, and substeps
- Uses `MapVisibilityChecker.TryRaycast` for wall/floor collisions
- Records trajectory points with minimum spacing from options

## Dependencies

- `MapVisibilityChecker`, `GrenadeSimulationOptions`, `GameOffsets`

## Configuration

`GrenadeSimulationOptions` (gravity, bounce, raycast substeps, throw speed scales, etc.).
