# ToolkitEventBus

## Purpose

Central in-process event bus. Decouples publishers (runtime, memory reader, input loop) from subscribers (overlays, future features).

## Events

| Event | Args type | Publisher |
|-------|-----------|-----------|
| `OnKeyDown` | `KeyInputEventArgs` | `ToolkitRuntime` |
| `OnKeyUp` | `KeyInputEventArgs` | `ToolkitRuntime` |
| `OnKeyPress` | `KeyInputEventArgs` | `ToolkitRuntime` |
| `OnMousePress` | `MouseInputEventArgs` | `ToolkitRuntime` |
| `OnMouseMove` | `MouseInputEventArgs` | `ToolkitRuntime` |
| `OnMemoryRead` | `MemoryReadEventArgs` | `GameMemoryReader` |
| `OnInjectionStatusChanged` | `InjectionStatusEventArgs` | `ToolkitRuntime` |

## Publish methods

- `PublishKeyDown` / `PublishKeyUp` / `PublishKeyPress`
- `PublishMousePress` / `PublishMouseMove`
- `PublishMemoryRead(MemoryState state)`
- `PublishInjectionStatus(InjectionStatus status, string message)`

## Registration

Registered as a **singleton** in DI. All services share one bus instance.

## Thread safety

Events are raised synchronously on the publisher's thread. Subscribers should keep handlers fast and marshal to the overlay thread only via `QueueDraw` (which is thread-safe).
