# GrenadeArcDrawer

## Purpose

Internal static helper that projects and draws grenade trail segments, bounce dots, and landing ground ring.

## Key API

- `Draw(Graphics, snapshot, viewMatrix, screenWidth, screenHeight, options)`

## Behavior

- Uses `WorldToScreenHelper.TryProject` for arc points
- Uses `TryProjectGroundRing` for landing marker circle
- Skips segments behind camera

## Dependencies

- [WorldToScreenHelper.md](WorldToScreenHelper.md)
- [GrenadeOverlay.md](GrenadeOverlay.md)
