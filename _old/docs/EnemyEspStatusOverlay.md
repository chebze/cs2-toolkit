# EnemyEspStatusOverlay

## Purpose

Bottom-left status label showing current enemy ESP mode after attach.

## Behavior

- Layer: `enemy-esp-status`, z-index `110`
- Subscribes to `OnMemoryRead`
- Labels: `ESP Off` (red), `ESP Last` (amber), `ESP Full` (green)
- Uses `Toolkit:Overlay:EspStatus` colors and margin

## Dependencies

- `EnemyEspState` — current mode
- `ScreenOverlayManager`
