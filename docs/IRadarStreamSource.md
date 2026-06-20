# IRadarStreamSource

## Purpose

API-layer port for radar snapshot and SSE stream endpoints. Extends `IRadarSnapshotProvider` from Services abstractions.

## Key API

Inherits from `IRadarSnapshotProvider`:

| Member | Description |
|--------|-------------|
| `Version` | Monotonic snapshot version for SSE change detection |
| `Changed` | Raised when the snapshot version advances |
| `GetSnapshot()` | Latest `RadarSnapshot` |
| `GetSnapshotJson()` | Camel-case JSON payload for SSE `data:` lines |

## Behavior

- Registered in `AddToolkitApi()` as an adapter over `IRadarSnapshotProvider`.
- The default `RadarState` implementation is updated by `RadarStateUpdater` on each game tick.

## Dependencies

- `IRadarSnapshotProvider` (Services abstractions)
- `RadarStreamSource` adapter (`CS2Toolkit.API`)

## Configuration

None.
