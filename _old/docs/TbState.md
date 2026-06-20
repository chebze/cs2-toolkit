# TbState

## Purpose

Thread-safe runtime state for triggerbot toggles and in-game adjustable parameters.

## Key API

| Member | Description |
|--------|-------------|
| `IsEnabled` | TB on/off |
| `IsAutoStopEnabled` | Counter-strafe before fire |
| `PreFireFovDegrees` | Angular pre-fire window |
| `MinReactionDelayMs` / `MaxReactionDelayMs` | Humanized delay range |
| `Toggle()` / `ToggleAutoStop()` | Runtime toggles |
| `AdjustPreFireFovDegrees(delta)` | FOV adjustment |
| `AdjustReactionDelays(deltaMs)` | Delay adjustment |

## Configuration

Initialized from `Toolkit:Tb`; FOV and delays persisted on save.
