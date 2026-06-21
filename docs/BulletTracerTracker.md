# BulletTracerTracker

## Purpose

Maintains active bullet tracer segments with TTL pruning and distance filtering for overlay rendering.

## Key API

- `Update(GameSnapshot snapshot, BulletTracerVisualOptions options)`
- `CopyState()` → `BulletTracerState`

## Behavior

- Resets when options are disabled or snapshot is detached / not in match
- Appends `GameSnapshot.RecentBulletImpacts` that pass kind filters (`ShowLocal`, `ShowTeammates`, `ShowEnemies`) and `MaxDistanceUnits` from local player
- Removes expired tracers after `DurationMs`
- Caps list length at `MaxActiveTracers` (oldest removed first)

## Dependencies

- `BulletTracerFeatureService` calls `Update` each tick
- `BulletTracerOverlayPresenter` reads `CopyState()`

## Configuration

Profile `Visuals.BulletTracers` (`BulletTracerVisualOptions`).
