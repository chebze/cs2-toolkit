# FeatureRegistry

## Purpose

Registers feature services and wires keybind actions to runtime feature toggles.

## Key API

Implements `IFeatureRegistry` and `IHostedService`.

## Behavior

| Keybind action | Effect |
|----------------|--------|
| `rcs-toggle` | Toggle RCS |
| `triggerbot-toggle` | Toggle triggerbot |
| `enemy-esp-toggle` | Cycle ESP mode (off → last seen → full) |
| `sound-esp-toggle` | Toggle sound ESP |
| `aimhelper-toggle` | Toggle aim helper |
| `triggerbot-auto-strafe` | Toggle TB auto-stop |
| `menu-toggle` | Toggle menu feature |
| `panic` | Detach, disable combat features, shutdown host |
| `save-settings` | Persist runtime toggles to active profile via `ProfileSettingsSaver` |

Inject/attach remains in Runtime `InjectKeybindOrchestrator`.

## Dependencies

- `IFeatureState`
- `IKeybindDispatcher`
- `ProfileSettingsSaver`
- `IRuntimeOrchestrator`
- `IGameAttachment`
- Registered `IFeatureService` instances

## Configuration

Key names from active profile `GlobalKeybinds`.
