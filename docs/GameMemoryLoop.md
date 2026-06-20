# GameMemoryLoop

## Purpose

Hosted background service that polls game memory at `MemoryReadIntervalMs` and publishes `GameSnapshot` values.

## Key API

Internal `BackgroundService` registered in `AddToolkitGame()`.

## Behavior

- Waits until `OffsetDownloader` has loaded offsets
- Builds snapshots via `GameSnapshotFactory` (entity reader, map name, view matrix, local player)
- Publishes through `GameStatePublisher` on each tick using `PeriodicTimer`
- When attached, logs a one-per-second summary: map name, player count, local weapon, in-match flag
- Catches and logs read failures without stopping the loop

## Dependencies

- `ProcessMemory`
- `OffsetDownloader`
- `GameStatePublisher`
- `ToolkitHostSettings` (`MemoryReadIntervalMs`, default 5)

## Configuration

`Toolkit:MemoryReadIntervalMs` in host `appsettings.json`.
