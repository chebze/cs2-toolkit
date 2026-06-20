# OverlayComposer

## Purpose

Merges all registered `IOverlayPresenter` layers into one `OverlayFrame` sorted by z-index.

## Key API

Implements `IOverlayComposer.Compose(snapshot, screenWidth, screenHeight)`.

## Behavior

- Returns empty frame when detached without active toasts
- Composes when status toasts are active even if detached from CS2
- Does **not** gate composition on `IsInMatch`; individual presenters decide whether to draw in lobby/menu vs in-round
- Status overlays (`FeatureStatusOverlayPresenter`, TB/RCS status presenters) can render while attached outside a match
- Sets `OverlayFrame.Interactive` when the menu feature is enabled
- Assigns incrementing `Sequence` per composed frame
- Logs a warning when a presenter exceeds a 1 ms budget

## Dependencies

- `IEnumerable<IOverlayPresenter>`
- `IWorldProjector`
- `IFeatureState`
- `IStatusToastPublisher`

## Configuration

None.
