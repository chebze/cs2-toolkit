# AimHelperReader

## Purpose

Reads aim-assist candidates from process memory: visible enemy bones with map line-of-sight and angular distance from the local view.

## Key API

| Member | Description |
|--------|-------------|
| `Read(state, eyePosition, viewAngles)` | Returns `AimHelperState` with head/neck/chest candidates |

## Behavior

- Skips when not attached, not in match, or eye position is invalid
- Uses enriched player bones and `IsVisibleToLocalPlayer` from `EntitySnapshotReader`
- Filters each bone through `MapVisibilityChecker` when map meshes are loaded
- Computes angular distance from local pitch/yaw to each bone

## Dependencies

- `ProcessMemory`, `GameOffsets`, `MapVisibilityChecker`, `LegacyMemoryState`

## Configuration

None.
