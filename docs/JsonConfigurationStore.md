# JsonConfigurationStore

## Purpose

JSON file persistence for profiles, keybinds, and web port (`data/configs/store.json`).

## Key API

Implements `IConfigurationStore` and `IConfigurationChangeNotifier`.

## Behavior

- Loads existing store on startup; migrates from `_old/data/configs/store.json`, then `_old/appsettings.json`, when missing
- On corrupt `store.json`: backs up to `store.json.corrupt.{timestamp}` and falls back to migration
- Thread-safe CRUD with atomic save (write temp + move)
- Raises `ConfigurationChanged` after mutations

## Dependencies

- `LegacySettingsMigrator`
- `IHostEnvironment` for content root paths
- `ILogger<JsonConfigurationStore>`

## Configuration

Store path: `{ContentRoot}/data/configs/store.json`
