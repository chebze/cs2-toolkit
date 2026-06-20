# RcsReader

## Purpose

Reads recoil-compensation inputs from process memory and maps them to `RcsState` on each game snapshot.

## Key API

| Member | Description |
|--------|-------------|
| `Read(isInMatch)` | Returns aim punch, shots fired, scoped flag, and whether punch data was read |

## Behavior

- Returns `RcsState.Inactive` when not attached, not in match, or local pawn is missing
- Reads `M_iShotsFired` and `M_bIsScoped` from the local player pawn when offsets are available
- Resolves latest aim punch from `M_pAimPunchServices` → `M_aimPunchCache` (last cache entry)
- Sets `HasAimPunch` false when punch offsets or cache data are unavailable

## Dependencies

- `ProcessMemory`, `GameOffsets`

## Configuration

None.
