# GameStatePublisher

## Purpose

Publishes mapped `GameSnapshot` instances to consumers via `IGameStateSource` and exposes the latest snapshot through `IReadOnlyGameState`.

## Key API

- `IGameStateSource.WatchAsync` — async stream of snapshots
- `IReadOnlyGameState.Latest` — most recently published snapshot
- `Publish(GameSnapshot)` — internal; called by `GameMemoryLoop`

## Behavior

- Uses an unbounded channel with a single writer (`GameMemoryLoop`)
- Overwrites `_latest` on each publish; channel consumers receive every published snapshot
- Does not block the memory loop on slow subscribers (`TryWrite`)

## Dependencies

- `CS2Toolkit.Models.Abstractions` (`GameSnapshot`, `IGameStateSource`, `IReadOnlyGameState`)

## Configuration

None.
