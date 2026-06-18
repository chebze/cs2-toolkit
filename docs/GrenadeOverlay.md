# GrenadeOverlay

## Purpose

Draws grenade trajectory arc, bounce markers, and landing ring on screen.

## Behavior

- Layer: `grenade-trajectory`, z-index `110`
- Subscribes to `OnMemoryRead` for z-order refresh
- Persistent `QueueDraw` handler projects `GrenadeTrajectoryTracker.Snapshot`
- Skips when `Overlay:GrenadeTrajectory:Enabled` is false or not in match

## Configuration

`Toolkit:Overlay:GrenadeTrajectory` — arc/landing colors, line widths, ring segments, diagnostics.

## Dependencies

- `GrenadeTrajectoryTracker`
- `ViewMatrixHolder`
- `GrenadeArcDrawer`
