# FeatureRuntimeState

## Purpose

Default `IFeatureState` implementation holding in-memory feature toggles for the running host.

## Key API

Implements all members of `IFeatureState`. See [IFeatureState.md](IFeatureState.md).

## Behavior

- Combat features default to disabled until `ApplyFromProfile` runs.
- `SetEnabled` for enemy ESP sets `LastSeen` when enabled, not `Full` (mode cycling is via `Toggle` / keybind).
- `ApplyFromProfile` maps profile DTO fields:
  - `Triggerbot.Global.Enabled`, `Rcs.Global.Enabled`, `AimHelper.Global.Enabled`
  - `SoundEsp.Enabled`
  - `EnemyEsp.Mode` (parsed case-insensitively; invalid → `Disabled`)
  - `Triggerbot.Global.AutoStopEnabled`
- `DisableAllCombatFeatures` clears all toggles and resets aim-helper hold state.

## Dependencies

- `IFeatureState` (implements)
- `ProfileSettings`, `FeatureIds`

## Configuration

None directly; values come from `ApplyFromProfile` and runtime keybinds.
