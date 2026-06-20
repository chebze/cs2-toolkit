# JsonConfigurationStore

## Purpose

JSON file persistence for profiles, keybinds, and web port (`data/configs/store.json`).

## Key API

Implements `IConfigurationStore` and `IConfigurationChangeNotifier`.

## Behavior

- Loads existing store on startup; migrates from legacy `appsettings.json` when missing
- Thread-safe CRUD with atomic save (write temp + move)
- Raises `ConfigurationChanged` after mutations

## Dependencies

- `LegacySettingsMigrator`
- `IHostEnvironment` for content root paths

## Configuration

Store path: `{ContentRoot}/data/configs/store.json`
