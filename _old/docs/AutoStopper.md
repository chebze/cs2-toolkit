# AutoStopper

## Purpose

Counter-strafes the local player to near-zero velocity before triggerbot fires when auto-stop is enabled.

## Behavior

- Reads local pawn velocity from memory
- Sends opposite movement keys via `SendInput` when speed exceeds threshold
- Used by [Triggerbot.md](Triggerbot.md) when `TbState.IsAutoStopEnabled` is true

## Configuration

`Toolkit:Tb:AutoStopEnabled`, `AutoStopSpeedThreshold`, `AutoStrafeKey`
