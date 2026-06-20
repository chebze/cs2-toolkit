# KeybindDispatcher

## Purpose

Maps physical key presses to toolkit keybind actions using definitions from `IKeybindConfiguration`.

## Key API

Implements `IKeybindDispatcher` as a `BackgroundService`.

Raises `KeybindActivated` when a configured binding is pressed.

## Behavior

- Waits for `StartupPhase.Overlay` in `ExecuteAsync` (does not block host `StartAsync`)
- Subscribes to `IInputListener.KeyDown` after the overlay phase completes
- Uses `IKeybindMatcher` for action lookup
- Logs activated bindings; feature toggles handled by `FeatureRegistry` (inject remains in Runtime)

## Dependencies

- `IInputListener`
- `IKeybindMatcher`
- `IRuntimeOrchestrator`
- `IKeybindConfiguration` (via matcher)

## Configuration

Key names from `GlobalKeybinds` in the active configuration store.
