# IGameLifecycle

## Purpose

Reports high-level game pipeline lifecycle state (offsets, attach, failure).

## Key API

- `GameLifecycleState State`
- `event Action<GameLifecycleState>? StateChanged`

States: `WaitingForOffsets`, `WaitingForGame`, `WaitingForAttach`, `Attached`, `Failed`.

## Behavior

Implemented by `GameAttachmentService`. `OffsetBootstrapHostedService` transitions to `WaitingForAttach` after offsets download.

## Dependencies

None (abstractions only).

## Configuration

None.
