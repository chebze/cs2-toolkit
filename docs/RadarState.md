# RadarState

## Purpose

Thread-safe holder for the latest `RadarSnapshot`, exposed to the web radar API and SSE stream (Phase 8).

## Key API

Implements `IRadarSnapshotProvider`.

| Member | Description |
|--------|-------------|
| `GetSnapshot()` / `GetSnapshotJson()` | Returns current radar data |
| `Update(snapshot)` | Replaces snapshot and increments version |
| `Version` | Monotonic counter for SSE clients |
| `Changed` | Fired after each update |

## Behavior

- Updated every tick by `RadarStateUpdater` from `GameSnapshot.Radar`
- JSON serialization uses camelCase property names for browser clients

## Dependencies

- `RadarSnapshot`, `IRadarSnapshotProvider`

## Configuration

None.
