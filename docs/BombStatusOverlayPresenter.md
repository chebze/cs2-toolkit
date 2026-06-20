# BombStatusOverlayPresenter

## Purpose

Renders the current bomb state on the overlay during active matches.

## Key API

Implements `IOverlayPresenter` with layer name `bomb-status`.

## Behavior

- Only draws when `GameSnapshot.IsInMatch` and `BombState.IsVisible`
- Formats status lines via `BombStatusFormatter` (carried, planting, planted, defusing, etc.)

## Dependencies

- `IActiveConfiguration`
- `BombStatusFormatter`, `OverlayTextBuilder`, `OverlayColorParser`

## Configuration

Profile setting `Visuals.BombStatus` (`TextPanelOverlayOptions`):

| Field | Default |
|-------|---------|
| `Enabled` | `true` |
| `X` | `16` |
| `Y` | `220` |
| `Color` | `#FFD166` |
| `FontSize` | `14` |

Legacy `Toolkit:Overlay:BombStatus` is migrated by `LegacySettingsMigrator`.
