# ToolkitRuntime

## Purpose

Central orchestrator hosted as a `BackgroundService`. Coordinates offset download, map parsing, injection flow, overlay startup, and the global input event loop.

## Lifecycle

```
Download offsets (fatal on failure)
    ↓
Start ScreenOverlayManager
    ↓
Parse map collision meshes (MapDataService)
    ↓
Signal RuntimeGate.OverlayReady
    ↓
Wait for cs2.exe process
    ↓
Show "Press {key} to inject..." (top-right)
    ↓
On inject key press → attach to CS2
    ↓
Signal RuntimeGate.InjectionComplete
    ↓
Run input event loop (16ms poll)
```

## Published events

Via `ToolkitEventBus`:

| Event | Trigger |
|-------|---------|
| `OnKeyDown` | Key transitions from up to down |
| `OnKeyUp` | Key transitions from down to up |
| `OnKeyPress` | Fired with `OnKeyDown` on each new press |
| `OnMousePress` | Mouse button newly pressed |
| `OnMouseMove` | Cursor position changed |
| `OnInjectionStatusChanged` | Injection flow state updates |

## Injection flow

1. **WaitingForGame** — polls until `cs2` process exists.
2. **WaitingForKeyPress** — draws prompt using the `system` overlay layer.
3. **Attaching** — calls `ProcessMemory.AttachToProcess("cs2")`.
4. **Attached** — signals `RuntimeGate` so `GameMemoryReader` can start.
5. **Failed** — throws `InvalidOperationException` (fatal).

The inject key is configured in `appsettings.json` under `Toolkit:InjectKey` (default `F9`).

## Panic key

Pressing `Toolkit:PanicKey` (default `F10`) at any point instantly shuts down the app:

1. Detaches from CS2 process memory
2. Closes the overlay window
3. Stops the host

## Map parsing

Before injection, calls `MapDataService.ParseAllMapsAsync` and reports progress on the system overlay layer. Map parsing failure is non-fatal; raycast features degrade gracefully when no meshes are available.

## System overlay layer

Uses `ScreenOverlayManager.GetOrCreateLayer("system", zIndex: 1000)` for injection prompts, map parsing status, and settings-save confirmations.

## Error handling

Fatal errors in `RunAsync` (offset download, invalid inject key, attach failure) are caught in `ExecuteAsync`, logged at critical level, set `Environment.ExitCode = 1`, and call `IHostApplicationLifetime.StopApplication()`.

## Dependencies

- `OffsetDownloader` — must succeed before anything else runs
- `MapDataService` — loads collision meshes for raycast features
- `ScreenOverlayManager` — started before injection UI
- `RuntimeGate` — gates `GameMemoryReader` startup
- `ToolkitEventBus` — input and injection status events
- `NativeInput` — global key/mouse polling
- `IHostApplicationLifetime` — graceful shutdown on fatal error
