# StatusToastOrchestrator

## Purpose

Hosted service that manages lifecycle toasts for attachment state (inject prompt, clear on match).

## Key API

`BackgroundService` — ticks on host memory read interval.

## Behavior

- When detached: sets persistent `Press {InjectKey} to inject...` toast
- When attached: clears persistent toast
- When in match: clears all toasts (game overlays take over)

## Dependencies

- `IReadOnlyGameState`, `IGameAttachment`, `IActiveConfiguration`, `IStatusToastPublisher`

## Configuration

Inject key from active profile keybinds.
