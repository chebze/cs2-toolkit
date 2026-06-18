# EnemyLastSeenTracker

## Purpose

Tracks per-enemy **last known skeleton** for the current round when any friendly player (including you) has spotted them.

## Detection

Each poll cycle:

1. Reset all data when `Round.RoundStartCount` changes
2. Build friendly player indices (your team, including local player)
3. For each living enemy:
   - Resolve pawn
   - Check `EntitySpottedState_t` on the pawn:
     - `m_bSpotted` is true, or
     - `m_bSpottedByMask` has a bit set for any friendly player index
   - Read bone positions via `BoneHelper`
   - Store/update `EnemyLastSeenSnapshot` keyed by player index

Spotted enemies that leave line of sight keep their last saved skeleton until they die or the round ends. Dead enemies are removed immediately and no longer drawn.

## Data

`EnemyLastSeenSnapshot` contains:

- `PlayerIndex`, `Name`
- `Bones` — 28 world positions (`PlayerBones.Count`)
- `LastSeenAt`

## Offsets

| Field | Source |
|-------|--------|
| `m_entitySpottedState` | `C_CSPlayerPawn` |
| `m_bSpotted` | `EntitySpottedState_t` + `0x8` |
| `m_bSpottedByMask` | `EntitySpottedState_t` + `0xC` |
| Bone array | `m_pGameSceneNode` → `m_modelState` + `0x80` |

## Wiring

`GameMemoryReader` calls `Initialize(offsets)` once, then `Poll(state)` each memory read.
