# GameMemoryReader

## Purpose

Hosted `BackgroundService` that polls CS2 process memory on a fixed interval, runs memory-tick features, and publishes the full game state via `OnMemoryRead`.

## Startup gating

Waits on `RuntimeGate.MemoryReaderStartTask`, which requires:

1. Overlay is ready (`SignalOverlayReady`).
2. Map parsing signaled complete.
3. Injection is complete (`SignalInjectionComplete`).

## Read loop

- Interval: `Toolkit:MemoryReadIntervalMs` (default **5ms**).
- Creates `EntityResolver` with downloaded offsets on first iteration.
- Initializes trackers and combat helpers once offsets are available.
- Each tick while attached:
  - Updates `ViewMatrixHolder` and active map on `MapVisibilityChecker`
  - Runs `Triggerbot.TryTrigger`, `RecoilCompensator.TryCompensate`, `AimHelper.TryAim`
  - Polls `EnemySoundTracker`, `EnemyLastSeenTracker`, `GrenadeTrajectoryTracker`
  - Publishes `MemoryState` through `ToolkitEventBus.PublishMemoryRead`

## Error handling

Read failures are logged as warnings and publish `MemoryState.Detached` rather than crashing the host.

## Published event

```
OnMemoryRead(MemoryReadEventArgs)
  └── State: MemoryState
```

Subscribers include stat overlays, status overlays, and `MatchLogger`.

## Dependencies

- `ProcessMemory` — must be attached before reads return meaningful data
- `OffsetDownloader.Offsets` — required for entity resolution
- `RuntimeGate` — controls when reading begins
- `MapDataService.VisibilityChecker` — raycast LOS for TB, aim helper, grenades
