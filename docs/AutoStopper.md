# AutoStopper

## Purpose

Counter-strafes movement keys to stop the local player before synthetic triggerbot fire.

## Key API

| Member | Description |
|--------|-------------|
| `TryEnsureStopped(input, triggerbot, options)` | Holds opposite WASD keys until speed is below threshold |
| `Reset(input)` | Releases all held counter keys |

## Dependencies

- `IInputSimulator`, `TriggerbotState`, `TriggerbotHostSettings`

## Configuration

`ToolkitHostSettings.Triggerbot.AutoStopSpeedThreshold` (default `15`).
