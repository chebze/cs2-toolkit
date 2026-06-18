# SoundEspState

## Purpose

Thread-safe runtime toggle for sound ESP (enemy ripples and bomb waves).

## Key API

| Member | Description |
|--------|-------------|
| `IsEnabled` | Whether sound ESP draws |
| `Initialize(SoundEspOptions)` | Sets from config |
| `Toggle()` | Flips enabled state |

## Configuration

`Toolkit:SoundEsp:Enabled` — initial state. Persisted on save.
