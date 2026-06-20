# RcsOverlayPresenter

## Purpose

Renders the RCS on/off status label in the bottom-left overlay stack.

## Key API

Implements `IOverlayPresenter` with layer name `rcs-status`.

## Behavior

- Shows green `RCS` when the feature is enabled, red when disabled
- Positioned one line above the triggerbot label (bottom margin + font size)
- Renders only when attached to the game process

## Dependencies

- `IFeatureState`, `RcsHostSettings`, `OverlayColorParser`

## Configuration

Host `Rcs`: `StatusFontSize`, `StatusMargin`, `EnabledColor`, `DisabledColor`.
