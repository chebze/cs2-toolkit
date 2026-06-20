# TriggerbotOverlayPresenter

## Purpose

Renders triggerbot status, pre-fire FOV circle, auto-stop indicator, and reaction delay labels.

## Key API

Implements `IOverlayPresenter` with layer name `triggerbot-status`.

## Behavior

- Bottom-left `TB` on/off label always visible when attached
- When enabled and in match: center FOV circle, auto-stop label, min/max reaction ms beside circle

## Dependencies

- `IActiveConfiguration`, `IFeatureState`, `TriggerbotHostSettings`, `FovCircleDrawBuilder`

## Configuration

Host `Triggerbot` (FOV circle color/width, assumed horizontal FOV, status font/margin); weapon `Triggerbot` layer for FOV and delays.
