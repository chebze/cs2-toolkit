# AimHelperController

## Purpose

Snapshot-driven aim assist. Selects the best visible enemy bone within FOV and moves the mouse toward its screen position.

## Key API

| Member | Description |
|--------|-------------|
| `Process(context)` | Evaluates candidates and applies relative mouse movement |

## Behavior

- Requires attached snapshot, in-match state, and optional activation key held (when configured)
- Picks target by preferred bone order (head → neck → chest variants) then lowest angular distance within FOV
- Projects target bone via `IWorldProjector` and moves mouse toward screen center
- Uses `IKeybindMatcher` to resolve the activation key from global keybinds

## Dependencies

- `FeatureContext`, `AimHelperState`, `IInputSimulator`, `IWorldProjector`, `IOverlayViewport`, `IKeybindMatcher`, `PreferredBoneParser`

## Configuration

Weapon profile `AimHelper` layer: `PreferredBone`, `FovDegrees`. Global `AimHelperActivationKey` (empty = always active when feature enabled).
