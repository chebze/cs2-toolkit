# GrenadeArcOverlayPresenter

## Purpose

Renders the local player's grenade throw arc preview on the overlay.

## Key API

Implements `IOverlayPresenter` with layer name `grenade-arc`.

## Behavior

- Draws when `Visuals.Grenade.Enabled` and `GameSnapshot.IsInMatch`
- Uses the first active entry in `GameSnapshot.Grenades`
- Delegates to `GrenadeArcDrawBuilder` at z-index 110

## Dependencies

- `IActiveConfiguration`, `ToolkitHostSettings.Grenade`
- `GrenadeArcDrawBuilder`, `IWorldProjector`

## Configuration

Profile `Visuals.Grenade` (`GrenadeVisualOptions`):

| Field | Default |
|-------|---------|
| `Enabled` | `true` |
| `ArcColor` | `#38BDF8` |
| `LandingColor` | `#FBBF24` |
| `ArcLineWidth` | `2` |
| `LandingLineWidth` | `1.5` |
| `LandingRingSegments` | `20` |

Legacy `Toolkit:Overlay:GrenadeTrajectory` settings migrate via `LegacySettingsMigrator`.
