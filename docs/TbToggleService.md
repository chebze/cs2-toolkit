# TbToggleService

## Purpose

Hosted service for triggerbot hotkey handling. See [Tb.md](Tb.md) for control reference.

## Controls

| Input | Action |
|-------|--------|
| Tap `ToggleKey` (default `F7`) | Toggle TB |
| Hold + arrows | Adjust FOV (left/right) or reaction delays (up/down) |

## Behavior

- `BackgroundService` with 16ms poll while toggle held for key repeat
- Releasing toggle after arrow adjustment does not flip enabled state

## Configuration

`Toolkit:Tb:ToggleKey`, FOV and delay adjust settings
