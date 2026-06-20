# OverlayFrame

## Purpose

Immutable snapshot of draw commands produced by the overlay pipeline for a single render tick.

## Key API

- `long Sequence` — monotonically increasing frame id
- `DateTimeOffset ProducedAt`
- `IReadOnlyList<DrawCommand> Commands`
- `bool Interactive` — when true, the renderer disables overlay click-through (menu open)

## Behavior

Published via `IOverlayFrameSink.Publish`; the WinForms renderer consumes the latest sequence only and drops older frames. `Interactive` is set when the in-game menu is visible.

## Dependencies

`DrawCommand` hierarchy in `CS2Toolkit.Drawing.Abstractions`.

## Configuration

None.
