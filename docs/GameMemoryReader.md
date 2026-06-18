# GameMemoryReader

## Purpose

Hosted `BackgroundService` that polls CS2 process memory on a fixed interval and publishes the full game state via `OnMemoryRead`.

## Startup gating

Waits on `RuntimeGate.MemoryReaderStartTask`, which requires:

1. Overlay is ready (`SignalOverlayReady`).
2. Injection is complete (`SignalInjectionComplete`).

This ensures stats overlays exist before memory events flow and that the process is attached.

## Read loop

- Interval: `Toolkit:MemoryReadIntervalMs` (default **100ms**).
- Creates `EntityResolver` with downloaded offsets on first iteration.
- Publishes `MemoryState` through `ToolkitEventBus.PublishMemoryRead`.

## Error handling

Read failures are logged as warnings and publish `MemoryState.Detached` rather than crashing the host.

## Published event

```
OnMemoryRead(MemoryReadEventArgs)
  └── State: MemoryState
```

Subscribers: `EnemyOverlay`, `TeammateOverlay`.

## Dependencies

- `ProcessMemory` — must be attached before reads return meaningful data
- `OffsetDownloader.Offsets` — required for entity resolution
- `RuntimeGate` — controls when reading begins
