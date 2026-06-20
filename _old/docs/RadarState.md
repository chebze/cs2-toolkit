# RadarState

## Purpose

Thread-safe holder for the latest `RadarSnapshot`, exposed to the web radar API and SSE stream.

## Key API

- `GetSnapshot()` / `GetSnapshotJson()`
- `Update(RadarSnapshot snapshot)`
- `Version` — monotonic counter for SSE clients
- `Changed` event

## Behavior

Updated every memory tick by `GameMemoryReader`. SSE endpoint polls version every 100ms.

## Dependencies

- `RadarSnapshot`

## Configuration

None.
