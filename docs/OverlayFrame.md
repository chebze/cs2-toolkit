# OverlayFrame

## Purpose

Immutable snapshot of draw commands produced by the overlay pipeline for a single render tick.

## Key API

- `long Sequence` — monotonically increasing frame id
- `DateTimeOffset ProducedAt`
- `IReadOnlyList<DrawCommand> Commands`

## Behavior

Published via `IOverlayFrameSink.Publish`; the WinForms renderer consumes the latest sequence only and drops older frames.

## Dependencies

`DrawCommand` hierarchy in `CS2Toolkit.Drawing.Abstractions`.

## Configuration

None.
