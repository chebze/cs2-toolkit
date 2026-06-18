# ViewMatrixHolder

## Purpose

Single source of truth for the game view-projection matrix used by all world-space overlays.

## Key API

| Method | Description |
|--------|-------------|
| `Initialize(GameOffsets)` | Stores offset for `dwViewMatrix` |
| `Update(ProcessMemory)` | Reads 16 floats from client memory each tick |
| `CopyTo(Span<float>)` | Thread-safe copy for projection |

## Behavior

- Updated every memory read while attached, independent of match/round state
- Uses a lock around the internal 16-float buffer
- Registered as singleton; injected into overlays and `AimHelper`

## Dependencies

- `ProcessMemory` — must be attached
- `GameOffsets.DwViewMatrix`
