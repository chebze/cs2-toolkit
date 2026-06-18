# GameOffsets

## Purpose

Strongly-typed container for CS2 memory offsets downloaded from cs2-dumper.

## Module offsets (client.dll RVAs)

| Field | Description |
|-------|-------------|
| `DwEntityList` | Pointer to entity list |
| `DwLocalPlayerPawn` | Local player pawn pointer |
| `DwLocalPlayerController` | Local player controller pointer |
| `DwGameRules` | Pointer to `C_CSGameRules` |
| `DwPlantedC4` | Pointer to planted C4 entity |
| `DwGlobalVars` | Pointer to global vars (`curtime` at `+0x2C`) |

## Schema field offsets

| Field | Class | Description |
|-------|-------|-------------|
| `M_hPlayerPawn` | `CCSPlayerController` | Handle to player pawn |
| `M_iTeamNum` | `C_BaseEntity` | Team identifier |
| `M_iHealth` | `C_BaseEntity` | Health value |
| `M_lifeState` | `C_BaseEntity` | Alive/dead life state |
| `M_iszPlayerName` | `CCSPlayerController` / `CBasePlayerController` | Player name string |
| `M_bHasMatchStarted` | `C_CSGameRules` | Whether a match is active |
| `M_bPawnIsAlive` | `CCSPlayerController` | Controller-side alive flag |
| `M_iPawnHealth` | `CCSPlayerController` | Controller-side pawn health |
| `M_sSanitizedPlayerName` | `CCSPlayerController` | Pointer to sanitized player name |
| `M_iConnected` | `CBasePlayerController` | Player connection state (`0` = skip) |
| `M_pWeaponServices` | `C_BasePlayerPawn` | Pointer to weapon services |
| `M_hActiveWeapon` | `CPlayer_WeaponServices` | Active weapon handle |
| `M_hMyWeapons` | `CPlayer_WeaponServices` | Weapon inventory vector |
| `M_AttributeManager` | `C_EconEntity` | Weapon econ attributes |
| `M_Item` | `C_AttributeContainer` | Item view container |
| `M_iItemDefinitionIndex` | `C_EconItemView` | Weapon ID (C4 = `49`) |
| `M_bBombDropped` | `C_CSGameRules` | Bomb dropped on ground |
| `M_bBombPlanted` | `C_CSGameRules` | Bomb planted |
| `M_nBombSite` | `C_PlantedC4` | Planted bombsite index |
| `M_flC4Blow` | `C_PlantedC4` | Detonation game time |
| `M_bBeingDefused` | `C_PlantedC4` | Defuse in progress |
| `M_flDefuseCountDown` | `C_PlantedC4` | Defuse completion game time |
| `M_bCannotBeDefused` | `C_PlantedC4` | Defuse blocked |
| `M_hBombDefuser` | `C_PlantedC4` | Defuser pawn handle |
| `M_bStartedArming` | `C_C4` | Plant animation started |
| `M_bIsPlantingViaUse` | `C_C4` | Plant via use key |
| `M_nWhichBombZone` | `C_CSPlayerPawn` | Bombsite zone index |
| `M_bIsDefusing` | `C_CSPlayerPawn` | Player defusing |
| `M_iBlockingUseActionInProgress` | `C_CSPlayerPawn` | Defuse with/without kit |
| `M_bPawnHasDefuser` | `CCSPlayerController` | Controller-side kit flag |
| `M_pItemServices` | `C_BasePlayerPawn` | Item services pointer |
| `M_bHasDefuser` | `CCSPlayer_ItemServices` | Pawn-side kit flag |

## Constants

| Constant | Value | Description |
|----------|-------|-------------|
| `EntitySpacings` | `0x70`, `0x78` | Bytes between entity slots in a chunk |
| `MaxPlayerIndex` | `64` | Maximum player index to iterate |
| `WeaponC4` | `49` | Item definition index for C4 |

## Source

Populated by `OffsetDownloader` at startup. Values change with CS2 game updates — always download fresh offsets.
