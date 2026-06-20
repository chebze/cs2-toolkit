# IRadarSnapshotProvider

## Purpose

Read-only port for the latest radar snapshot. Consumed by the API layer for `/api/radar/snapshot` and SSE streaming.

## Key API

| Member | Description |
|--------|-------------|
| `GetSnapshot()` | Current `RadarSnapshot` |
| `GetSnapshotJson()` | JSON-serialized snapshot (camelCase) |
| `Version` | Monotonic update counter |
| `Changed` | Notifies when version advances |

## Behavior

Implemented by `RadarState` in Services. API endpoints poll `Version` or subscribe to `Changed` for SSE push.

## Dependencies

- `RadarSnapshot`

## Configuration

None.
