# IFeatureRegistry

## Purpose

Central registry of feature services with runtime enable/disable control.

## Key API

- `Features` — all registered `IFeatureService` instances
- `TryGet`, `IsEnabled`, `SetEnabled`, `Toggle`

## Behavior

Implemented by `FeatureRegistry`, which also subscribes to `IKeybindDispatcher` and maps hotkeys to feature toggles (Phase 7.3.1).

## Dependencies

- `IFeatureState`
- `IKeybindDispatcher` (abstractions only)

## Configuration

Hotkey action ids from `ToolkitKeybindActions`.
