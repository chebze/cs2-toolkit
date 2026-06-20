# TriggerbotController

## Purpose

Snapshot-driven triggerbot state machine with synthetic fire and optional auto-stop.

## Key API

| Member | Description |
|--------|-------------|
| `Process(context, autoStopEnabled)` | Evaluates acquisition and fires/releases via `IInputSimulator` |
| `Reset(input)` | Releases synthetic mouse and counter-movement keys |

## Behavior

- Phases: Idle → PreFire → OnTarget → PostFire with grace bullet budgets
- Reaction delay randomized from weapon-layer min/max ms settings
- Skips when user holds left mouse, reloading, or not in match
- Auto-stop counter-strafes (W/A/S/D) until horizontal speed drops below threshold

## Dependencies

- `FeatureContext`, `TriggerbotState`, `AutoStopper`, `IInputSimulator`

## Configuration

Weapon profile `Triggerbot` layer + host `Triggerbot` settings (grace bullets, auto-stop threshold).
