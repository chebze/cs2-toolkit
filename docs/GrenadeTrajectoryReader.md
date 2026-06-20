# GrenadeTrajectoryReader

## Purpose

Maps resolved grenade trajectory diagnostics to `GrenadeState` on each game snapshot.

## Key API

| Member | Description |
|--------|-------------|
| `Read(isInMatch)` | Returns active `GrenadeState` or `GrenadeState.Inactive` |

## Behavior

- Delegates to `GrenadeTrajectoryResolver` each tick
- Converts internal snapshot to public `GrenadeState` for `GameSnapshot.Grenades`

## Dependencies

- `GrenadeTrajectoryResolver`, `ProcessMemory`, `GameOffsets`, `MapVisibilityChecker`

## Configuration

None directly; uses host `GrenadePhysicsSettings`.
