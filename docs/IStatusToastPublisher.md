# IStatusToastPublisher

## Purpose

Publishes ephemeral and persistent status toasts for the top-right system message overlay.

## Key API

| Member | Description |
|--------|-------------|
| `Publish(message, duration?, colorArgb?)` | Adds a timed toast (default 4s when duration omitted for timed - actually we use explicit durations from callers) |
| `SetPersistent(message, colorArgb?)` | Replaces the single persistent toast until cleared |
| `Clear()` / `ClearPersistent()` | Remove toasts |
| `GetActive()` / `HasActive` | Read active toasts for rendering |

## Behavior

- Timed toasts expire automatically when `ExpiresAt` is reached
- Persistent toasts remain until cleared or replaced
- Implemented by `StatusToastStore` as a singleton

## Dependencies

- `StatusToast` model

## Configuration

Toast colors default to profile `Visuals.SystemMessages.Color` when `colorArgb` is zero.
