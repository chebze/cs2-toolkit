# BulletTracerOverlayPresenter

## Purpose

Renders fading bullet tracer lines for local players, teammates, and enemies.

## Key API

Implements `IOverlayPresenter` with layer name `bullet-tracers`.

## Behavior

- Draws when profile `Visuals.BulletTracers.Enabled`, runtime `FeatureIds.BulletTracers` toggle is on, and `GameSnapshot.IsInMatch`
- Delegates to `BulletTracerDrawBuilder` at z-index 105
- Line color selected by `BulletTracerKind` (`LocalColor`, `TeammateColor`, `EnemyColor`)
- Alpha fades out over `DurationMs`

## Dependencies

- `IActiveConfiguration`, `IFeatureState`, `BulletTracerTracker`
- `BulletTracerDrawBuilder`, `IWorldProjector`

## Configuration

Profile `Visuals.BulletTracers`. Toggle keybind: global `BulletTracersToggleKey` (default `F3`).
