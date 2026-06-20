# AimHelperState

## Purpose

Thread-safe runtime state for aim helper enabled flag and FOV.

## Key API

| Member | Description |
|--------|-------------|
| `IsEnabled` | Whether aim helper runs |
| `FovDegrees` | Current angular target window |
| `PreferredBone` | Target bone preference |
| `Initialize(AimHelperOptions)` | Loads config |
| `Toggle()` | Flips enabled |
| `AdjustFovDegrees(float delta)` | Clamps to min/max FOV |

## Configuration

`Toolkit:AimHelper` — persisted on save for `Enabled` and `FovDegrees`.
