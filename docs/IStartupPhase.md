# IStartupPhase

## Purpose

Gate for a single `StartupPhase` value. Completed by `IRuntimeOrchestrator.CompletePhase`.

## Key API

| Member | Description |
|--------|-------------|
| `Phase` | The `StartupPhase` enum value |
| `IsComplete` | Whether the gate has been signaled |
| `WaitAsync(CancellationToken)` | Awaits completion |

## Behavior

- Implemented internally by `StartupPhaseGate` in `CS2Toolkit.Runtime`.
- Exposed via `IRuntimeOrchestrator.GetPhase`.

## Dependencies

- `IRuntimeOrchestrator`

## Configuration

None.
