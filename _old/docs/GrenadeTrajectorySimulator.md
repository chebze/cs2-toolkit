# GrenadeTrajectorySimulator

## Purpose

Tick-based grenade physics simulation with map collision bounces.

## Key API

| Method | Description |
|--------|-------------|
| `Simulate(...)` | Full arc from throw state to rest |
| `TryComputeThrowState(...)` | Static helper for initial velocity from view angles |

## Behavior

- Integrates velocity with gravity each tick (`TickIntervalSeconds`, default 1/64s)
- Sub-step raycasts along motion against map meshes
- Applies bounce elasticity and stop velocity threshold
- Respects `MaxSimulationTicks`, `MaxBounces`, trail point limits

## Configuration

`Toolkit:Grenade` — gravity, bounce, raycast steps, spacing, throw speed scales.

## Dependencies

- `MapVisibilityChecker` — line and ground queries
