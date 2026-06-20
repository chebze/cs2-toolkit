# FeatureCoordinator

## Purpose

Hosted pipeline orchestrator: run enabled features, then compose and publish overlay frames.

## Key API

Internal `BackgroundService` registered in `AddToolkitServices()`.

## Behavior

- Waits for `IRuntimeOrchestrator` **Attach** phase before ticking
- Polls at `MemoryReadIntervalMs` (same cadence as game memory loop)
- Order per tick: acquire `IProfileRuntimeSync` → build `FeatureContext` → `IFeatureService.OnSnapshot` → `IOverlayComposer.Compose` → `IOverlayFrameSink.Publish`
- Profile switch / toggle apply blocks ticks until the runtime sync lock is released
- Feature failures are logged; overlay publish continues
- Marks `StartupPhase.Features` complete when started

## Dependencies

- `IReadOnlyGameState`
- `IActiveConfiguration`
- `IEnumerable<IFeatureService>`
- `IInputSimulator`
- `IOverlayComposer`, `IOverlayFrameSink`, `IOverlayViewport`
- `IRuntimeOrchestrator`
- `IProfileRuntimeSync`

## Configuration

`Toolkit:MemoryReadIntervalMs`
