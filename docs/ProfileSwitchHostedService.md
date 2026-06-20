# ProfileSwitchHostedService

## Purpose

Listens for per-profile `SwitchHotkey` key presses and switches the active configuration profile at runtime.

## Key API

Hosted service (`IHostedService`); no public methods.

## Behavior

- Builds a `virtualKey → profileId` map from all profiles with a non-empty `SwitchHotkey`.
- Rebuilds the map on `ConfigurationChanged` (e.g. hotkey edits in the config UI).
- Logs a warning when two profiles share the same `SwitchHotkey` (last profile wins).
- On `IInputListener.KeyPress`, calls `IActiveProfileSwitcher.SwitchTo` when the key matches.
- Shows a short status toast with the new profile name; failures show a red error toast.
- `ActiveProfileSwitcher` applies the new profile toggles under `IProfileRuntimeSync`.

## Dependencies

- `IInputListener`
- `IKeybindMatcher`
- `IConfigurationStore`
- `IConfigurationChangeNotifier`
- `IActiveProfileSwitcher`
- `IStatusToastPublisher`
- `ILogger<ProfileSwitchHostedService>`

## Configuration

Per-profile `SwitchHotkey` in `store.json` (same format as other keybind strings).
