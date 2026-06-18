# Triggerbot

## Purpose

Automatically fires when the crosshair is on an enemy with humanized timing. See [Tb.md](Tb.md) for full feature guide.

## Key API

| Method | Description |
|--------|-------------|
| `Initialize(offsets, options, mapChecker?)` | One-time setup |
| `TryTrigger(memory, clientBase, state, enabled, fov, minDelay, maxDelay, autoStop)` | Called each memory tick |

## Behavior

- Pre-fire window uses angular FOV to spotted visible enemies
- On-target uses `m_iIDEntIndex` crosshair entity
- Random reaction delay and grace bullets per acquisition
- Optional `AutoStopper` when `autoStop` is true
- Synthetic mouse via `SendInput`

## Configuration

`Toolkit:Tb`

## Dependencies

- `TbState` — runtime enabled, FOV, delays, auto-stop
- `MapVisibilityChecker` — pre-fire visibility
