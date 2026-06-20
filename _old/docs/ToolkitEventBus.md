# ToolkitEventBus

## Purpose

Central in-process event bus. Decouples publishers (runtime, memory reader, input loop) from subscribers (overlays, toggle services).

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

`EnemySoundTracker.OnEnemyNoise` is a separate direct event (not on the bus).

## Publish methods

- `PublishKeyDown` / `PublishKeyUp` / `PublishKeyPress`
- `PublishMousePress` / `PublishMouseMove`
- `PublishMemoryRead(MemoryState state)`
- `PublishInjectionStatus(InjectionStatus status, string message)`

## Subscribers (representative)

| Subscriber | Events |
|------------|--------|
| `MenuOverlay` | `OnKeyPress`, `OnMouseMove` |
| `EnemyOverlay`, stat overlays | `OnMemoryRead` |
| `RcsToggleService`, `TbToggleService`, `AimHelperToggleService` | `OnKeyDown`, `OnKeyUp` |
| `EnemyEspToggleService`, `SoundEspToggleService` | `OnKeyPress` |
| `SettingsSaveService` | `OnKeyPress` |

## Registration

Registered as a **singleton** in DI. All services share one bus instance.

## Thread safety

Events are raised synchronously on the publisher's thread. Subscribers should keep handlers fast and marshal to the overlay thread only via `QueueDraw` (which is thread-safe).
