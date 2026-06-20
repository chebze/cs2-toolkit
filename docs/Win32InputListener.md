# Win32InputListener

## Purpose

Polls global keyboard and mouse state on a background loop and raises platform-neutral input events.

## Key API

Implements `IInputListener`, `IInputState`, and runs as an `IHostedService`.

Events: `KeyDown`, `KeyUp`, `KeyPress`, `MouseMove`, `MouseDown`.

## Behavior

- Poll interval: 16 ms
- Tracks held keys/buttons to emit edge-triggered events
- Uses `GetAsyncKeyState` via `Win32InputNative`

## Dependencies

None (Win32 P/Invoke).

## Configuration

None.
