# TbOverlay

## Purpose

Bottom-left `TB` status label and crosshair FOV ring with reaction delay labels when triggerbot is enabled in-match.

## Behavior

- Layer: `tb-status`, z-index `110`
- Draws red FOV circle and min/max ms labels via `DrawHelper`
- Subscribes to `OnMemoryRead`

## Dependencies

- [TbState.md](TbState.md)
- Feature guide: [Tb.md](Tb.md)
