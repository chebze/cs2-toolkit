# SoundEspWaveTracker

## Purpose

Maintains active sound ESP wave indicators from mapped `GameSnapshot.RecentSounds` and planted/defusing bomb positions.

## Key API

| Member | Description |
|--------|-------------|
| `Update(snapshot, options)` | Ingests new sound events and updates bomb wave state |
| `CopyState()` | Thread-safe snapshot of active waves for overlay drawing |

## Behavior

- Resets immediately when `options.Enabled` is false (allows cleanup ticks while sound ESP is off)
- Adds a wave for each `SoundEvent` within `MaxDistanceUnits` of the local player
- Expires waves after `WaveDurationMs`
- Loops a continuous wave at the bomb world position while status is `Planted` or `Defusing`
- Clears all waves when detached or not in match

## Dependencies

- `GameSnapshot`, `SoundEspProfileOptions`, `BombState`

## Configuration

Profile `SoundEsp`: `WaveDurationMs`, `MaxDistanceUnits`.
