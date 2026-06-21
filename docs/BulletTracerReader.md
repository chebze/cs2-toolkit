# BulletTracerReader

## Purpose

Detects new shots from all alive players by tracking `M_iShotsFired` per pawn and emitting tracer segments for the current tick.

## Key API

- `Detect(LegacyMemoryState state)` → `IReadOnlyList<BulletImpactEvent>`

## Behavior

- Clears internal shot baselines when not attached or not in match
- For each alive player: resolve pawn, compare `M_iShotsFired` to previous value
- On increase: read eye position (`origin + m_vecViewOffset`) and aim direction (`m_angEyeAngles`)
- Seeds `M_iShotsFired` baseline on first sight per player without emitting (avoids spurious tracers mid-spray)
- Traces along aim vector via `MapVisibilityChecker.TryRaycast` (max 8192 units); falls back to open-air endpoint when no map index is loaded
- Classifies tracers as local, teammate, or enemy from player team vs local team
- Caps burst detection at 12 bullets per tick per player to avoid runaway queues after desync

## Dependencies

- `ProcessMemory`, `GameOffsets`
- `MapVisibilityChecker`
- `BombSiteHelper` for entity origin

## Configuration

None. Trace distance is fixed in the reader.
