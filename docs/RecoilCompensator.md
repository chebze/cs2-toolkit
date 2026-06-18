# RecoilCompensator

## Purpose

Applies mouse movement opposite to aim punch while spraying. See [Rcs.md](Rcs.md) for feature guide.

## Key API

| Method | Description |
|--------|-------------|
| `Initialize(offsets, options)` | One-time setup |
| `TryCompensate(memory, clientBase, enabled)` | Called each memory tick |

## Behavior

- Reads aim punch cache and `m_iShotsFired` from local pawn
- Humanized random skip per bullet (configurable chances)
- Only active while left mouse held, not scoped, enabled, and after second bullet

## Configuration

`Toolkit:Rcs`

## Dependencies

- `NativeInput` — synthetic mouse move
- `RcsState` — runtime enabled flag
