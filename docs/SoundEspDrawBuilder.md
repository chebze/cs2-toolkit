# SoundEspDrawBuilder

## Purpose

Builds overlay draw commands for sound ESP ground rings or static boxes.

## Key API

| Member | Description |
|--------|-------------|
| `Build(state, options, projector, viewMatrix, width, height, zIndex)` | Returns `DrawCommand` list for active waves and bomb indicator |

## Behavior

- **Waves** animation: expanding ground rings with fade-out alpha (ported from `GroundWaveDrawer`)
- **StaticBox** animation: centered rectangle at projected ground position
- Projects ground rings by sampling points around the world X/Y circle at the event Z

## Dependencies

- `SoundEspWaveState`, `SoundEspProfileOptions`, `IWorldProjector`
- `PolylineDrawCommand`, `RectDrawCommand`, `OverlayColorParser`

## Configuration

Profile `SoundEsp`: `Animation`, `WaveColor`, `WaveLineWidth`, `WaveDurationMs`, `MinWorldRadius`, `MaxWorldRadius`, `RingCount`, `RingSpacing`.
