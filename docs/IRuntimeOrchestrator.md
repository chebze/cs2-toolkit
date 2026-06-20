# IRuntimeOrchestrator

## Purpose

Coordinates ordered toolkit startup phases and exposes gates for hosted services to await.

## Key API

| Member | Description |
|--------|-------------|
| `CurrentPhase` | Latest completed phase |
| `IsFailed` / `Failure` | Fatal bootstrap state |
| `GetPhase(StartupPhase)` | Per-phase gate (`IStartupPhase`) |
| `CompletePhase(StartupPhase)` | Marks a phase complete and unblocks waiters |
| `WaitForPhaseAsync(StartupPhase, CancellationToken)` | Awaits a phase gate |
| `Fail(Exception)` | Fails all gates and records the error |
| `RequestShutdown(string)` | Stops the generic host (panic / fatal paths) |

## Behavior

- Phases complete in order: `Offsets` → `Maps` → `Overlay` → `Input` → `Attach` → `GameLoop` → `Features` → `Api`.
- `RuntimeOrchestratorHostedService` completes through `Attach`; game loop, features, and API mark their own phases when they start.
- Gated services (`GameMemoryLoop`, `FeatureCoordinator`, `ApiHostService`, etc.) call `WaitForPhaseAsync` before doing work.

## Dependencies

- Implemented by `RuntimeOrchestrator` in `CS2Toolkit.Runtime`
- Registered in `AddRuntimeOrchestration()`

## Configuration

None.
