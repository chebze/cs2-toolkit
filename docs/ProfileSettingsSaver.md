# ProfileSettingsSaver

## Purpose

Persists runtime feature toggle state into the active configuration profile (`data/configs/store.json`).

## Key API

| Method | Description |
|--------|-------------|
| `SaveActiveProfile()` | Merges `IFeatureState` into the active profile and calls `IConfigurationStore.UpdateProfile` |

## Behavior

- Triggered by the save-settings keybind (`F11` by default) via `FeatureRegistry`.
- Writes global-layer enabled flags, enemy ESP mode, sound ESP enabled, and triggerbot auto-stop to the active profile.
- Does not write to legacy `appsettings.json`.

## Dependencies

- `IConfigurationStore`
- `IFeatureState`

## Configuration

Uses keybind `saveSettingsKey` from the configuration store (default `F11`).
