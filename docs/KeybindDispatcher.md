# KeybindDispatcher

## Purpose

Maps physical key presses to toolkit keybind actions using definitions from `IKeybindConfiguration`.

## Key API

Implements `IKeybindDispatcher` and `IHostedService`.

Raises `KeybindActivated` when a configured binding is pressed.

## Behavior

- Subscribes to `IInputListener.KeyDown`
- Uses `IKeybindMatcher` for action lookup
- Logs activated bindings; feature registry wiring arrives in Phase 7

## Dependencies

- `IInputListener`
- `IKeybindMatcher`
- `IKeybindConfiguration` (via matcher)

## Configuration

Key names from `GlobalKeybinds` in the active configuration store.
