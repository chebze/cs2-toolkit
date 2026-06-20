# OverlayComposer

## Purpose

Merges all registered `IOverlayPresenter` layers into one `OverlayFrame` sorted by z-index.

## Key API

Implements `IOverlayComposer.Compose(snapshot, screenWidth, screenHeight)`.

## Behavior

- Returns empty frame when detached or not in match
- Assigns incrementing `Sequence` per composed frame

## Dependencies

- `IEnumerable<IOverlayPresenter>`
- `IWorldProjector`

## Configuration

None.
