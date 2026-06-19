# RuntimeConfigProvider

## Purpose

Holds the active runtime `ToolkitOptions` snapshot and `ProfileSettings`, rebuilt when the config store changes. Resolves weapon-layered settings for triggerbot, RCS, and aim helper.

## Key API

- `Current` — active `ToolkitOptions`
- `ActiveProfile` / `ActiveSettings`
- `ResolveTriggerbot(weaponId)`, `ResolveRcs(weaponId)`, `ResolveAimHelper(weaponId)`
- `ConfigChanged` event

## Behavior

- Subscribes to `ConfigManager.StoreChanged`
- Thread-safe reads via internal lock
- Weapon resolution delegates to `WeaponSettingsResolver`

## Dependencies

- `ConfigManager`, `WeaponSettingsResolver`, `LayeredWeaponSettings`
