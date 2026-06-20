# TeammateStatsOverlayPresenter

## Purpose

Renders teammate alive/dead counts on the overlay during active matches.

## Key API

Implements `IOverlayPresenter` with layer name `teammate-stats`.

## Behavior

- Only draws when `GameSnapshot.IsInMatch` is true
- Uses `TeammatesAlive` and `TeammatesDead` from the snapshot (local player excluded in game mapping)
- Respects `Profile.Visuals.TeammateStats.Enabled`

## Display

```
Teammates
  Alive: x
  Dead:  x
```

## Dependencies

- `IActiveConfiguration`
- `OverlayTextBuilder`, `OverlayColorParser`

## Configuration

Profile setting `Visuals.TeammateStats` (`TextPanelOverlayOptions`):

| Field | Default |
|-------|---------|
| `Enabled` | `true` |
| `X` | `16` |
| `Y` | `120` |
| `Color` | `#6BCB77` |
| `FontSize` | `14` |

Legacy `Toolkit:Overlay:TeammateStats` is migrated by `LegacySettingsMigrator`.
