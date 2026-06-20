# StatusToastStore

## Purpose

Thread-safe in-memory store for status toasts consumed by `StatusToastOverlayPresenter`.

## Key API

Implements `IStatusToastPublisher`.

## Behavior

- Holds one optional persistent toast plus a list of timed toasts
- Prunes expired timed toasts on read
- Default color `0` means use presenter theme color

## Dependencies

- `StatusToast`, `IStatusToastPublisher`

## Configuration

None.
