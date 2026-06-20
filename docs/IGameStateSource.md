# IGameStateSource

## Purpose

Publishes a stream of `GameSnapshot` values to consumers (feature coordinator, radar, debug tooling).

## Key API

- `WatchAsync(CancellationToken)` — `IAsyncEnumerable<GameSnapshot>`

## Behavior

Implementations publish snapshots on the game poll interval. Consumers must not block the publisher.

## Dependencies

Implemented by `CS2Toolkit.Game` (`GameStatePublisher`, Phase 4).

## Configuration

Poll interval from `ToolkitHostSettings.MemoryReadIntervalMs`.
