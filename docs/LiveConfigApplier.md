# LiveConfigApplier

## Purpose

Applies configuration changes live to runtime state, memory subsystems, and overlay styling without restarting the toolkit.

## Key API

- `IHostedService.StartAsync` — subscribes to `RuntimeConfigProvider.ConfigChanged` and applies immediately

## Behavior

On config change:

- Re-initializes `TbState`, `RcsState`, ESP states, `AimHelperState`
- Updates `OverlayStyleState` for draw colors/options
- Re-initializes `RecoilCompensator`, `Triggerbot`, `AimHelper` when offsets are available

## Dependencies

- `RuntimeConfigProvider`, `ConfigManager`
- Feature state objects and memory classes
