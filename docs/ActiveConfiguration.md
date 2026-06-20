# ActiveConfiguration

## Purpose

Runtime source of truth for resolved toolkit settings from the active profile, keybinds, and host options.

## Key API

- `Current` — `ToolkitSettings`
- `ResolveWeapon(ushort weaponId)` — layered weapon settings
- `Refresh()` — rebuild after store or host options change

Implements `IActiveConfiguration` and `IKeybindConfiguration`.

## Behavior

Subscribes to `IConfigurationChangeNotifier` and `IOptionsMonitor<ToolkitHostSettings>`.

## Dependencies

- `IConfigurationStore`
- `ISettingsResolver`

## Configuration

Host options: `Toolkit` section in `appsettings.json`. Feature settings: active profile in configuration store.
