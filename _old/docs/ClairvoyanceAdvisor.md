# ClairvoyanceAdvisor

## Purpose

Evaluates contextual gameplay tips from live memory each read cycle.

## Tips

| Tip | Condition |
|-----|-----------|
| `You should reload` | Active weapon clip is `<= LowAmmoClipThreshold` (knives/C4 excluded) |
| `You should be sneaky` | Any living enemy is within `EnemyCloseDistanceUnits` |
| `They're probably going A/B` | CT only; more enemies are within `BombsiteEnemyRadiusUnits` of one site than the other |
| `They're probably planting A/B` | CT only; carrier is in a bomb zone (`m_bInBombZone`) and nearest site center is A or B; or dropped C4 is closer to one site; or bomb overlay already reports planting with a site |
| `We should plant on site A/B` | T only; bomb not planting/planted/defusing; more enemies are within `BombsiteEnemyRadiusUnits` of one site than the other |
| `They're about to defuse...` | T only; bomb planted; local player outside `BombsiteEnemyRadiusUnits` of planted site; at least one enemy inside that radius |
| `No tips yet...` | None of the above conditions match |

## Data sources

- Local pawn position via `m_vOldOrigin`
- Enemy pawn positions via controller → pawn resolution
- Bombsite centers from map rules entity via `BombSiteHelper` (`m_bombsiteCenterA/B`, `m_foundGoalPositions`)
- Plant site label: nearest center to active C4 weapon position, else carrier pawn position (not `m_nWhichBombZone`)
- Active weapon clip via `m_iClip1`
- Bomb position from T carrier pawn or `dwWeaponC4` when dropped

## Configuration

`Toolkit:Clairvoyance` in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `LowAmmoClipThreshold` | `3` | Clip count that triggers reload advice |
| `EnemyCloseDistanceUnits` | `600` | Distance for sneaky warning |
| `BombsiteEnemyRadiusUnits` | `1500` | Radius for enemy site grouping |
