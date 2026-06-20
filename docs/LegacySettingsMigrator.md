# LegacySettingsMigrator

## Purpose

One-time migration from legacy configuration formats into v2 `ConfigurationStore`.

## Key API

| Method | Description |
|--------|-------------|
| `TryMigrateLegacyStore(IHostEnvironment)` | Imports `_old/data/configs/store.json` when present |
| `MigrateFromLegacyAppSettings(IHostEnvironment)` | Maps legacy `Toolkit` section from `appsettings.json` |

## Behavior

- `JsonConfigurationStore` calls `TryMigrateLegacyStore` before appsettings migration when `data/configs/store.json` does not exist.
- Appsettings migration reads `appsettings.json` in the content root, falling back to `_old/appsettings.json`.
- Produces a default profile with mapped triggerbot, RCS, aim helper, ESP, sound, overlay, and keybind fields.

## Dependencies

- `Configuration.Abstractions` DTOs
- `ConfigurationJson.Options` for deserialization

## Configuration

Legacy paths: `_old/data/configs/store.json`, `_old/appsettings.json`, host `appsettings.json`.
