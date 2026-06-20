# MenuOverlayPresenter

## Purpose

Renders the read-only in-game settings menu when the menu feature is toggled on (default key: Insert).

## Key API

Implements `IOverlayPresenter` with layer name `menu` and z-index `500`.

## Behavior

- Shows when `IFeatureState` has menu enabled via `menu-toggle` keybind
- Displays current keybinds, memory interval, enemy ESP, and teammate stats settings
- Background panel drawn via `MenuPanelDrawBuilder`
- When visible, `OverlayComposer` sets `OverlayFrame.Interactive` so the renderer disables click-through

## Dependencies

- `IActiveConfiguration`, `IFeatureState`, `MenuPanelDrawBuilder`, `OverlayColorParser`

## Configuration

Profile `Visuals.Menu`: `X`, `Y`, `BackgroundColor`, `TextColor`, `FontSize`.
