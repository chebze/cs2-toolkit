# ToolkitEvents

## Purpose

Event argument types and enums used by `ToolkitEventBus`.

## KeyInputEventArgs

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `Keys` | The keyboard key |
| `Timestamp` | `DateTime` | UTC time of the event |

## MouseInputEventArgs

| Property | Type | Description |
|----------|------|-------------|
| `Button` | `MouseButtons` | Mouse button (or `None` for move) |
| `X`, `Y` | `int` | Screen coordinates |
| `Timestamp` | `DateTime` | UTC time of the event |

## MemoryReadEventArgs

| Property | Type | Description |
|----------|------|-------------|
| `State` | `MemoryState` | Full game memory snapshot |
| `Timestamp` | `DateTime` | UTC time of the read |

## InjectionStatusEventArgs

| Property | Type | Description |
|----------|------|-------------|
| `Status` | `InjectionStatus` | Current injection phase |
| `Message` | `string` | Human-readable status text |

## InjectionStatus enum

| Value | Description |
|-------|-------------|
| `WaitingForGame` | CS2 process not found |
| `WaitingForKeyPress` | Waiting for inject key |
| `Attaching` | Attempting process attach |
| `Attached` | Successfully attached |
| `Failed` | Attach failed |
