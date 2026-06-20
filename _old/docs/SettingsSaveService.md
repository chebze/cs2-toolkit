# SettingsSaveService

## Purpose

Hosted service that persists runtime toggle state to `appsettings.json` when the save key is pressed.

## Behavior

- Binds `Toolkit:SaveSettingsKey` (default `F11`) via `OnKeyPress`
- Collects state from `RcsState`, `TbState`, `EnemyEspState`, `SoundEspState`, `AimHelperState`
- Writes full `Toolkit` section via `AppSettingsWriter`
- Shows confirmation on system overlay layer
- Empty/invalid save key disables the service

## Dependencies

- `ToolkitOptionsCollector` — merges config + runtime state
- `AppSettingsWriter` — atomic JSON write
- `IHostEnvironment.ContentRootPath` — `appsettings.json` location
