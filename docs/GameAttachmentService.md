# GameAttachmentService

## Purpose

Attaches to the CS2 process and reports lifecycle state through `IGameLifecycle`.

## Key API

Implements `IGameAttachment` and `IGameLifecycle`.

- `TryAttach(processName)` — opens `cs2` (default) and resolves `client.dll`
- `Detach()` — releases the process handle
- `State` / `StateChanged` — offset load, attach, and failure phases

## Behavior

- Delegates process I/O to internal `ProcessMemory`
- Starts in `WaitingForOffsets`; `OffsetBootstrapHostedService` moves to `WaitingForAttach` after offsets load
- Successful attach sets `Attached`; failure sets `Failed`
- Inject keybind in Runtime calls `TryAttach` (see `InjectKeybindOrchestrator`)

## Dependencies

- `ProcessMemory`
- `Microsoft.Extensions.Logging`

## Configuration

Process name defaults to `"cs2"`; inject key comes from active profile keybinds (`GlobalKeybinds.InjectKey`).
