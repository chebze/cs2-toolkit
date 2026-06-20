# ClairvoyanceAdvisor

## Purpose

Evaluates contextual gameplay tips from live memory each read cycle and maps them to `GameSnapshot.ClairvoyanceTips`.

## Key API

| Member | Description |
|--------|-------------|
| `ResolveTips(...)` | Returns contextual tip strings for the current match state |

## Behavior

Tips are team-aware and bombsite-aware:

| Tip | Condition |
|-----|-----------|
| `You should reload` | Active weapon clip ≤ threshold (knives/C4 excluded) |
| `You should be sneaky` | Any living enemy within close distance |
| `They're probably going A/B` | CT; more enemies near one bombsite |
| `They're probably planting A/B` | CT; carrier in bomb zone or dropped C4 near a site |
| `We should plant on site A/B` | T; bomb not planting/planted/defusing; more enemies near one site |
| `They're about to defuse...` | T; bomb planted; local outside site radius; enemy inside |
| `No tips yet...` | No conditions matched |

## Dependencies

- `ProcessMemory`, `GameOffsets`, `BombSiteHelper`, `LegacyPlayerInfo`, `LegacyBombInfo`, `LegacyBombSitesInfo`

## Configuration

Host `Clairvoyance`: `LowAmmoClipThreshold`, `EnemyCloseDistanceUnits`, `BombsiteEnemyRadiusUnits`.
