# EnemySoundTracker

## Purpose

Detects nearby enemy sound events from live pawn memory and raises `OnEnemyNoise` for the overlay to render.

## Detection

Each poll cycle (50ms):

1. Read `dwViewMatrix` for world-to-screen projection
2. For each living enemy within `MaxDistanceUnits` (default 2000) of the local player:
   - Compare `m_flEmitSoundTime` and `m_nLastJumpTick` to the previous snapshot
   - On change, classify the noise and fire `OnEnemyNoise`

| Sound type | Classification |
|------------|----------------|
| `Reload` | Active weapon `m_bInReload` is true |
| `Jump` | `m_nLastJumpTick` changed |
| `Step` | Emit time changed while `m_bIsWalking` |
| `Other` | Emit time changed with no clearer signal |

The first snapshot per enemy is stored without firing, so existing sounds do not spam waves on attach.

## Event

```csharp
event EventHandler<EnemyNoiseEventArgs>? OnEnemyNoise;
```

`EnemyNoiseEventArgs` carries `PlayerIndex`, `SoundType`, `WorldPosition` (feet at detection time), and `Timestamp`.

## Configuration

`Toolkit:EnemyNoise` in `appsettings.json`.

## Wiring

`GameMemoryReader` calls `Initialize(offsets)` once, then `Poll(state)` before each `OnMemoryRead` publish.
