# GroundWaveDrawer

## Purpose

Internal helper that draws expanding concentric ground ripples for sound ESP and planted bomb animation.

## Key API

- `DrawRings(graphics, centerWorld, progress, viewMatrix, screenWidth, screenHeight, options)`

## Behavior

- Projects horizontal rings at feet height via `WorldToScreenHelper.TryProjectGroundRing`
- Fades alpha by ring progress
- Used by [EnemyNoiseOverlay.md](EnemyNoiseOverlay.md)

## Configuration

`Toolkit:EnemyNoise` — duration, radii, ring count, color
