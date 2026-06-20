# OverlayComposer

## Purpose

Merges all registered `IOverlayPresenter` layers into one `OverlayFrame` sorted by z-index.

## Key API

Implements `IOverlayComposer.Compose(snapshot, screenWidth, screenHeight)`.

## Behavior

- Returns empty frame when detached without active toasts
- Composes when status toasts are active even if detached from CS2
- Sets `OverlayFrame.Interactive` when the menu feature is enabled
- Assigns incrementing `Sequence` per composed frame

## Dependencies

- `IEnumerable<IOverlayPresenter>`
- `IWorldProjector`
- `IFeatureState`
- `IStatusToastPublisher`

## Configuration

None.
