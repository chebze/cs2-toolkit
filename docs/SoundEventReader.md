# SoundEventReader

## Purpose

Detects enemy footstep, reload, jump, and other sound events from process memory and maps them to `SoundEvent` records on each game snapshot.

## Key API

| Member | Description |
|--------|-------------|
| `Detect(entityList, localPawn, localTeam, players, isInMatch)` | Returns new `SoundEvent` instances detected since the previous tick |

## Behavior

- Maintains per-player sound snapshots across ticks (`EmitSoundTime`, walking, reload, jump tick)
- Emits events when emit time or jump tick changes (ported from legacy `EnemySoundTracker`)
- Classifies sounds as `Step`, `Reload`, `Jump`, or `Other`
- Clears state when not in match or when local team is unknown

## Dependencies

- `ProcessMemory`, `GameOffsets`, `BombSiteHelper`
- `SoundEvent`, `SoundKind`, `PlayerId`, `Vector3`

## Configuration

None. Distance filtering is applied in `SoundEspWaveTracker` using profile `MaxDistanceUnits`.
