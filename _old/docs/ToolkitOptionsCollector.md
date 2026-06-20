# ToolkitOptionsCollector

## Purpose

Static helper that merges baseline `ToolkitOptions` from config with live runtime state for persistence.

## Key API

```csharp
ToolkitOptions Collect(
    ToolkitOptions config,
    RcsState rcsState,
    TbState tbState,
    EnemyEspState enemyEspState,
    SoundEspState soundEspState,
    AimHelperState aimHelperState)
```

## Behavior

Deep-clones config via JSON serialize/deserialize, then overwrites:

- `Rcs.Enabled`
- `Tb.Enabled`, `AutoStopEnabled`, `PreFireFovDegrees`, reaction delays
- `EnemyEsp.Mode`
- `SoundEsp.Enabled`
- `AimHelper.Enabled`, `FovDegrees`

## Dependencies

Used by [SettingsSaveService.md](SettingsSaveService.md).
