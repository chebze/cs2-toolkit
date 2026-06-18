# GrenadeTrajectory

## Purpose

Models for grenade arc data consumed by overlay drawing.

## Types

### `GrenadeTrajectorySource`

| Value | Meaning |
|-------|---------|
| `Game` | Trail read from game entity memory |
| `Simulated` | Physics simulation fallback |

### `GrenadeTrajectorySnapshot`

| Property | Description |
|----------|-------------|
| `IsActive` | Whether to draw |
| `Source` | Game vs simulated |
| `Trail` | World-space arc points |
| `BouncePoints` | Bounce locations |
| `LandingPoint` | Final rest position |

## Dependencies

Produced by [GrenadeTrajectoryResolver.md](GrenadeTrajectoryResolver.md); drawn by [GrenadeOverlay.md](GrenadeOverlay.md).
