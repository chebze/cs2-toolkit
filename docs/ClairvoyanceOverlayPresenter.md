# ClairvoyanceOverlayPresenter

## Purpose

Renders contextual clairvoyance tips from `GameSnapshot.ClairvoyanceTips` as a text panel overlay.

## Key API

Implements `IOverlayPresenter` with layer name `clairvoyance`.

## Behavior

- Shows a `Clairvoyance` header followed by indented tip lines when in match
- No feature toggle — visibility controlled by profile panel `Enabled` flag
- Always reads tips already computed in the Game layer

## Dependencies

- `IActiveConfiguration`, `OverlayTextBuilder`, `OverlayColorParser`

## Configuration

Profile `Visuals.Clairvoyance` (`Enabled`, `X`, `Y`, `Color`, `FontSize`).
