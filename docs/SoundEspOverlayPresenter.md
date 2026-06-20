# SoundEspOverlayPresenter

## Purpose

Renders enemy sound ESP wave indicators and bomb sound rings on the overlay.

## Key API

Implements `IOverlayPresenter` with layer name `sound-esp`.

## Behavior

- Draws when profile `SoundEsp.Enabled`, runtime feature toggle is on, and `GameSnapshot.IsInMatch`
- Reads active waves from `SoundEspWaveTracker`
- Delegates to `SoundEspDrawBuilder` at z-index 200

## Dependencies

- `IActiveConfiguration`, `IFeatureState`, `SoundEspWaveTracker`
- `SoundEspDrawBuilder`, `IWorldProjector`

## Configuration

Profile `SoundEsp` (`SoundEspProfileOptions`):

| Field | Default |
|-------|---------|
| `Enabled` | `true` |
| `Animation` | `Waves` |
| `WaveColor` | `#E53935` |
| `WaveDurationMs` | `900` |
| `MaxDistanceUnits` | `2000` |

Legacy `Toolkit:EnemyNoise` settings are migrated by `LegacySettingsMigrator`.
