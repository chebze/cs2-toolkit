# TriggerbotReader

## Purpose

Reads triggerbot acquisition state from process memory and maps it to `TriggerbotState` on each game snapshot.

## Key API

| Member | Description |
|--------|-------------|
| `Read(state)` | Returns crosshair target, reload/shots/velocity, and nearest visible enemy angle |

## Behavior

- Detects crosshair-on-enemy via `M_iIDEntIndex`
- Scans visible enemies for nearest angular distance within line of sight (map raycast)
- Clears to `TriggerbotState.Inactive` when not in match

## Dependencies

- `ProcessMemory`, `GameOffsets`, `MapVisibilityChecker`, `LegacyMemoryState`

## Configuration

None.
