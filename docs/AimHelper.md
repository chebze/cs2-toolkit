# AimHelper

## Purpose

Memory-tick aim assist that snaps view angles toward the nearest visible enemy bone within a configurable FOV.

## Key API

| Method | Description |
|--------|-------------|
| `Initialize(offsets, options, mapChecker, viewMatrixHolder)` | One-time setup |
| `TryAim(memory, clientBase, state, enabled, fovDegrees, preferredBone)` | Called each memory tick |

## Behavior

- Skips when disabled, not in match, scoped conditions fail, or optional `ActivationKey` is not held
- Requires enemy spotted-by-local-player (`m_entitySpottedState`)
- Optional map raycast LOS via `MapVisibilityChecker`
- Picks best bone by angular distance from crosshair using `PreferredBone` preference order
- Writes adjusted `m_angEyeAngles` on the local pawn

## Configuration

`Toolkit:AimHelper` — see [ToolkitOptions.md](ToolkitOptions.md) and [AimHelperToggleService.md](AimHelperToggleService.md).

## Dependencies

- `ViewMatrixHolder`, `BoneHelper`, `MapVisibilityChecker`, `NativeInput`
