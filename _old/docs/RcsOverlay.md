# RcsOverlay

## Purpose

Bottom-left `RCS` status label (green enabled / red disabled).

## Behavior

- Layer: `rcs-status`, z-index `110`
- Subscribes to `OnMemoryRead`
- Uses `Toolkit:Overlay:RcsStatus` margin and colors

## Dependencies

- [RcsState.md](RcsState.md)
- Feature guide: [Rcs.md](Rcs.md)
