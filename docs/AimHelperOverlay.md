# AimHelperOverlay

## Purpose

Bottom-left `AIM` status label and crosshair FOV ring when aim helper is enabled in-match.

## Behavior

- Layer: `aim-helper-status`, z-index `110`
- Draws FOV circle via `DrawHelper.GetAngularFovCircleLayout`
- Shows bone preference letter and FOV degrees
- Uses `Toolkit:Overlay:AimHelperStatus` and `Toolkit:AimHelper` colors

## Dependencies

- `AimHelperState`
- `OnMemoryRead` for refresh
