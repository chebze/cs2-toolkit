# IFeatureState

## Purpose

Runtime toggle state for combat and overlay features. Keybinds and the config UI mutate this state; `ProfileSettingsSaver` persists it back to the active profile.

## Key API

| Member | Description |
|--------|-------------|
| `IsEnabled(FeatureId)` | Whether a feature is active (enemy ESP uses `EnemyEspMode != Disabled`) |
| `SetEnabled(FeatureId, bool)` | Sets a boolean feature; enemy ESP maps `true` → `LastSeen` |
| `Toggle(FeatureId)` | Toggles a feature; enemy ESP cycles modes |
| `EnemyEspMode` | Current enemy ESP mode (`Disabled`, `LastSeen`, `Full`) |
| `CycleEnemyEspMode()` | Cycles enemy ESP through modes |
| `TriggerbotAutoStopEnabled` | Runtime TB auto-stop toggle |
| `ToggleTriggerbotAutoStop()` | Flips TB auto-stop |
| `AimHelperActivationHeld` | Transient hold state for aim-helper activation |
| `DisableAllCombatFeatures()` | Panic: turns off all combat features |
| `ApplyFromProfile(ProfileSettings)` | Hydrates toggles from a saved profile |

## Behavior

- Implemented by `FeatureRuntimeState` (singleton).
- `FeatureStateHydrator` calls `ApplyFromProfile` on startup and whenever configuration changes (profile switch, API save, F11).
- Keybind handlers and `FeatureRegistry` update state at runtime; F11 / `ProfileSettingsSaver` writes state to disk.
- Menu feature is always forced off when applying a profile (`ApplyFromProfile` sets menu to `false`).

## Dependencies

- `ProfileSettings` from `CS2Toolkit.Configuration.Abstractions`
- `FeatureId`, `EnemyEspMode` from `CS2Toolkit.Models.Abstractions`

## Configuration

Hydrated from the active profile in `store.json`: global TB/RCS/aim enabled flags, enemy ESP mode, sound ESP enabled, TB auto-stop.
