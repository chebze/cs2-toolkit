# EnemyEspOverlayPresenter

## Purpose

Renders enemy ESP overlays (last-seen or full skeletons) on the game window.

## Key API

Implements `IOverlayPresenter` with layer name `enemy-esp`.

## Behavior

- Draws only when `IFeatureState.EnemyEspMode` is not `Disabled` and `GameSnapshot.IsInMatch`
- Reads drawable targets from `EnemyEspTracker` for the active mode
- Delegates drawing to `EnemyEspDrawBuilder` using profile `EnemyEsp` colors and toggles

## Dependencies

- `IActiveConfiguration`, `IFeatureState`, `EnemyEspTracker`
- `EnemyEspDrawBuilder`, `IWorldProjector`

## Configuration

Profile `EnemyEsp` (`EnemyEspProfileOptions`):

| Field | Default |
|-------|---------|
| `Mode` | `LastSeen` (runtime mode is keybind-driven) |
| `SkeletonColor` | `#FF6B6B` |
| `BoundingBoxColor` | `#FF6B6B` |
| `SkeletonLineWidth` | `1.5` |
| `ShowBoundingBox` | `false` |
| `ShowPlayerName` | `false` |
| `ShowPlayerHealth` | `false` |
