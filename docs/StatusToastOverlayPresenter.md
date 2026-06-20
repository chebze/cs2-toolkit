# StatusToastOverlayPresenter

## Purpose

Renders active status toasts in the top-right corner on overlay layer `system` (z-index 1000).

## Key API

Implements `IOverlayPresenter`.

## Behavior

- Reads active toasts from `IStatusToastPublisher`
- Positions text via `TopRightTextDrawBuilder`
- Visible even when detached from CS2 (inject prompt)

## Dependencies

- `IActiveConfiguration`, `IStatusToastPublisher`, `TopRightTextDrawBuilder`, `OverlayColorParser`

## Configuration

Profile `Visuals.SystemMessages`: `Margin`, `Color`, `FontSize`.
