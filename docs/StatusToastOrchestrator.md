# StatusToastOrchestrator

## Purpose

Hosted service that manages lifecycle toasts for attachment state (inject prompt, clear on match).

## Key API

`BackgroundService` — ticks on host memory read interval.

## Behavior

- When detached: does not set inject prompts (owned by `RuntimeOrchestratorHostedService`)
- Waits for `StartupPhase.Input` before ticking
- When attached: clears persistent toast
- When in match: clears all toasts (game overlays take over)

## Dependencies

- `IReadOnlyGameState`, `IGameAttachment`, `IStatusToastPublisher`, `IRuntimeOrchestrator`
