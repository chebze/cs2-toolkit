# EnemyNoiseOverlay

## Purpose

Renders expanding ripple rings at enemy feet when `EnemySoundTracker` fires `OnEnemyNoise`.

## Behavior

- Layer: `enemy-noise` at z-index `200` (above stat panels)
- Subscribes to `EnemySoundTracker.OnEnemyNoise`
- Each event spawns a wave anchored to the frozen world position where the sound occurred
- Each frame (~60 FPS), projects a **horizontal ground ring** (circle in the XY plane at feet height) and draws fading concentric ripples

## Wave appearance

All ripples use `WaveColor` (default red `#E53935`) at `WaveLineWidth` (default `1`).

While the bomb is **planted** or **defusing**, the same ground ripple animation loops continuously at the bomb's world position.

| Sound | Notes |
|-------|-------|
| Enemy noise | One-shot ripples on `OnEnemyNoise` |
| Planted bomb | Continuous looping ripples at `BombInfo.WorldPosition` |

Rings expand from `MinWorldRadius` to `MaxWorldRadius` in **world units** along the ground plane over `WaveDurationMs` (default 900ms). `RingCount` and `RingSpacing` control how many ripples trail each event.

## Configuration

`Toolkit:EnemyNoise` in `appsettings.json`.

| Setting | Default | Description |
|---------|---------|-------------|
| `MaxDistanceUnits` | `2000` | Tracker range (overlay only draws events the tracker emits) |
| `WaveDurationMs` | `900` | How long each ripple animation lasts |
| `MinWorldRadius` | `10` | Starting ground ripple radius in world units |
| `MaxWorldRadius` | `90` | Ending ground ripple radius in world units |
| `WaveLineWidth` | `1` | Ring stroke width in pixels |
| `WaveColor` | `#E53935` | Ripple color (enemy sounds and planted bomb) |
| `RingCount` | `3` | Concurrent rings per event |
| `RingSpacing` | `0.22` | Progress offset between rings |
