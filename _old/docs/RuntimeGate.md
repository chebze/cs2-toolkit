# RuntimeGate

## Purpose

Synchronization primitive that coordinates startup order between `ToolkitRuntime` and `GameMemoryReader`.

## Signals

| Method | When called | Effect |
|--------|-------------|--------|
| `SignalOverlayReady()` | After overlay window is created | Unblocks overlay wait |
| `SignalInjectionComplete()` | After successful CS2 attach | Unblocks injection wait |

## Tasks

| Task | Completes when |
|------|----------------|
| `OverlayReadyTask` | Overlay is ready |
| `InjectionCompleteTask` | Injection succeeded |
| `MemoryReaderStartTask` | Both overlay and injection are done |

## Usage

`GameMemoryReader` awaits `MemoryReaderStartTask` before entering its 100ms read loop. This guarantees:

1. Overlay layers exist for drawing.
2. `ProcessMemory` is attached to CS2.

## Registration

Singleton in DI. Idempotent — repeated signals are ignored.
