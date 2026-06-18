# AimHelperToggleService

## Purpose

Hosted service for aim helper hotkey handling.

## Controls

| Input | Action |
|-------|--------|
| Tap `ToggleKey` (default `F4`) | Toggle enabled |
| Hold `ToggleKey` + `←` / `→` | Decrease / increase FOV |

FOV adjustments set a flag so releasing the toggle key after adjusting does not also toggle enabled state.

## Behavior

- Polls `NativeInput` at 16ms while toggle key held for arrow repeat
- Invalid toggle key throws at startup

## Configuration

`Toolkit:AimHelper:ToggleKey`, `FovAdjustStepDegrees`, `FovAdjustRepeatIntervalMs`
