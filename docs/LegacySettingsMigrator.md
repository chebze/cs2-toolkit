# LegacySettingsMigrator

## Purpose

One-time migration from legacy configuration formats into v2 `ConfigurationStore`.

## Key API

| Method | Description |
|--------|-------------|
| `TryMigrateLegacyStore(IHostEnvironment)` | Imports `_old/data/configs/store.json` when present |
| `MigrateFromLegacyAppSettings(IHostEnvironment)` | Maps legacy `Toolkit` section from `appsettings.json` |

## Behavior

- `TryMigrateLegacyStore` imports `_old/data/configs/store.json` when present
- `MigrateFromLegacyAppSettings` prefers `_old/appsettings.json` over v2 host `appsettings.json` for feature/keybind fields
- Produces a default profile with mapped triggerbot, RCS, aim helper, ESP, sound, overlay, and keybind fields.

## Dependencies

- `Configuration.Abstractions` DTOs
- `ConfigurationJson.Options` for deserialization

## Configuration

Legacy paths: `_old/data/configs/store.json`, `_old/appsettings.json`, host `appsettings.json`.
