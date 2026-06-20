# ActiveProfileSwitcher

## Purpose

Atomically applies active profile settings to runtime state (`IActiveConfiguration` + `IFeatureState`) and coordinates profile switches from hotkeys, API, and store-driven changes.

## Key API

| Member | Description |
|--------|-------------|
| `SwitchTo(profileId)` | Sets active profile in the store, refreshes resolved settings, hydrates toggles |
| `ApplyActiveProfileToggles(settings)` | Refreshes config cache and applies toggle fields after an active-profile save |

Also registered as `IHostedService` for startup hydration.

## Behavior

- Uses `IProfileRuntimeSync` to exclude `FeatureCoordinator` ticks during profile apply/switch.
- Subscribes to `ConfigurationChanged` and hydrates only when the active profile **id** changes (delete/default switch, etc.).
- Skips re-entrant hydration while `SwitchTo` is in progress (`_applying` guard).
- Does **not** reset runtime toggles on `UpdateProfile`, keybind edits, or F11 save.
- Config UI `PUT /api/configs/{id}` on the active profile calls `ApplyActiveProfileToggles` only when toggle fields in the request differ from the persisted profile.

## Dependencies

- `IConfigurationStore`
- `IActiveConfiguration`
- `IFeatureState`
- `IConfigurationChangeNotifier`
- `IProfileRuntimeSync`
- `ILogger<ActiveProfileSwitcher>`

## Configuration

None.
