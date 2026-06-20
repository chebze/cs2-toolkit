# RuntimeOrchestratorHostedService

## Purpose

`BackgroundService` that runs the sequential bootstrap replacing legacy `ToolkitRuntime` + `RuntimeGate`.

## Startup sequence

1. Download offsets (fatal on failure) → `Offsets`
2. Wait for overlay renderer ready → `Overlay`
3. Preload map collision meshes → `Maps`
4. Signal input ready → `Input`
5. Show persistent inject toast; wait for `IGameLifecycle` attach → `Attach`
6. Unblock `GameMemoryLoop`, `FeatureCoordinator`, and radar updater (they complete `GameLoop` / `Features` when started)
7. `ApiHostService` starts after `Maps` and completes `Api`

## Key API

Implements `BackgroundService.ExecuteAsync`.

## Behavior

- Uses `IStatusToastPublisher` for map-parse and inject prompts.
- On unhandled failure: `IRuntimeOrchestrator.Fail`, `Environment.ExitCode = 1`, host stop.

## Dependencies

- `IRuntimeOrchestrator`, `IOffsetProvider`, `IOverlayRenderer`, `MapDataService`
- `IGameLifecycle`, `IGameAttachment`, `IStatusToastPublisher`, `IHostApplicationLifetime`

## Configuration

None (host settings consumed by dependencies).
