# AimHelperOverlayPresenter

## Purpose

Renders aim helper status, FOV circle, and preferred-bone label.

## Key API

Implements `IOverlayPresenter` with layer name `aim-helper-status`.

## Behavior

- Bottom-left `AIM` on/off label (third line above margin, below TB and RCS)
- When enabled and in match: center FOV circle with degree and bone labels beside the circle

## Dependencies

- `IActiveConfiguration`, `IFeatureState`, `AimHelperHostSettings`, `FovCircleDrawBuilder`, `PreferredBoneParser`

## Configuration

Host `AimHelper` (status colors/font/margin, FOV circle color/width, assumed horizontal FOV); weapon `AimHelper` layer for FOV and preferred bone.
