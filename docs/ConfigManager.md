# ConfigManager

## Purpose

Manages multiple configuration profiles, global keybinds, and persistence in `data/configs/store.json`. Migrates legacy `appsettings.json` Toolkit settings on first run.

## Key API

- `GetStore()` / `GetActiveProfile()` / `GetProfile(id)`
- `CreateProfile`, `UpdateProfile`, `DeleteProfile`
- `SetActiveProfile`, `SetDefaultProfile`
- `UpdateKeybinds`, `UpdateWebPort`
- `ExportProfile`, `ImportProfile`
- `BuildToolkitOptions()` — maps active profile + global keybinds to `ToolkitOptions`

## Behavior

- Atomic JSON writes via `.tmp` + move
- Raises `StoreChanged` on any mutation (triggers live reload)
- Active profile is used at runtime; default profile loads on startup

## Dependencies

- `IHostEnvironment` for content root paths
- `ToolkitOptions`, `ConfigStore`, `ConfigProfile`, `ProfileSettings`

## Configuration

- Store file: `data/configs/store.json`
- Legacy migration source: `appsettings.json` (`Toolkit` section)
