# SoundEspStatusOverlay

## Purpose

Bottom-left `S-ESP` status label (green enabled / red disabled).

## Behavior

- Layer: `sound-esp-status`, z-index `110`
- Subscribes to `OnMemoryRead`
- Uses `Toolkit:Overlay:SoundEspStatus` styling

## Dependencies

- `SoundEspState`
