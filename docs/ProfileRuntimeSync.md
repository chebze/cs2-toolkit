# ProfileRuntimeSync

## Purpose

Default `IProfileRuntimeSync` implementation using a monitor lock.

## Key API

Implements `IProfileRuntimeSync.Acquire()`.

## Behavior

- `Acquire()` calls `Monitor.Enter` and returns a releaser that calls `Monitor.Exit` on dispose.
- Same-thread re-entry is supported (nested `Acquire` on the same thread).

## Dependencies

- `IProfileRuntimeSync` (implements)

## Configuration

None.
