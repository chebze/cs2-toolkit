# EntityResolver

## Purpose

Translates raw CS2 memory into a structured `MemoryState` with match detection, player enumeration, and team stat aggregation.

## Stat resolution

### Alive counts (primary)

Counted from the player controller list: teammates/enemies where `m_bPawnIsAlive` is true or `m_iPawnHealth > 0`.

### Dead counts

Derived from the player controller list using `m_bPawnIsAlive == false` and `m_iPawnHealth <= 0`.

### Fallback

If no controllers are found, alive counts fall back to `C_CSGameRules.m_iMatchStats_PlayersAlive_CT/T`.

## Player enumeration

Controllers are collected via:

1. Entity index loop `1..highestEntityIndex` with chunk lookup
2. Flat controller list at `entityList + 0x10`
3. Both `0x70` and `0x78` entity spacings attempted

A valid controller must have a non-zero `m_hPlayerPawn` handle.

## Team resolution order

1. Pawn `m_iTeamNum`
2. Controller `m_iPendingTeamNum`
3. Controller `m_iTeamNum` (C_BaseEntity offset)

## Alive detection

Controller fields (`m_bPawnIsAlive`, `m_iPawnHealth`) are only reliably replicated for **teammates**.

| Player | Primary source | Fallback |
|--------|----------------|----------|
| Teammates | Controller pawn fields | Pawn `m_iHealth` |
| Enemies | Pawn `m_iHealth` + `m_lifeState` | Controller fields |

Pawn is resolved trying both `0x70` and `0x78` entity spacing. Dead when `health <= 0` or pawn `m_lifeState` (byte) equals `2` (LIFE_DEAD).

## Entity filtering

Player scan is limited to indices `1..64` (not `highestEntityIndex`). A controller must:

- Have a resolvable pawn handle
- Have `m_iConnected` not disconnected (`4`) or never-connected (`0xFFFFFFFF`)
- Have a valid player name (letters/digits, no `prefab` strings)
- Match local player only by controller pointer equality

## Bomb status

Resolved from `C_CSGameRules`, `C_PlantedC4`, player pawns, and C4 weapon state:

| Status | Detection |
|--------|-----------|
| `Planted` | `m_bBombPlanted` on game rules |
| `Defusing` | `m_bBeingDefused` on planted C4 or CT defuse action in progress |
| `Planting` | C4 active weapon with `m_bStartedArming` / `m_bIsPlantingViaUse` |
| `OnGround` | `m_bBombDropped` on game rules |
| `Equipped` | C4 (`m_iItemDefinitionIndex == 49`) is the active weapon |
| `Carried` | C4 is in `m_hMyWeapons` but not active |
| `None` | No bomb state available |

### Site and timers

| Field | Source |
|-------|--------|
| Bombsite centers | `m_bombsiteCenterA` / `m_bombsiteCenterB` on map rules entity (`BombSiteHelper`) |
| Site while planting | Nearest center to active C4 weapon position, else carrier pawn position |
| Site when planted | Nearest center to planted C4 world position |
| World position | Planted C4 entity origin (`BombSiteHelper`) — used for ground wave overlay |
| Time left | `m_flC4Blow - curtime` via `dwGlobalVars` |
| Defuse time | `m_flDefuseCountDown - curtime` (with fallbacks) |
| Kit | Defuser `m_iBlockingUseActionInProgress`, `m_bHasDefuser`, or `m_bPawnHasDefuser` |
| Will succeed | bomb time left >= defuse time remaining (absolute defuse end vs `m_flC4Blow` as fallback) |

`m_nWhichBombZone` and `m_nBombSite` are map-specific entity indices, not A/B labels — do not use them for site names.

## Match detection

Requires local pawn, team T/CT, and `m_bHasMatchStarted` when game rules are readable.
