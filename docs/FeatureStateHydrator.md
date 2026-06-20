# FeatureStateHydrator

## Purpose

Keeps `IFeatureState` in sync with the active configuration profile on startup and after profile or settings changes.

## Key API

Hosted service (`IHostedService`); no public methods.

## Behavior

- On `StartAsync`, subscribes to `IConfigurationChangeNotifier.ConfigurationChanged` and immediately applies the current profile.
- `ApplyFromProfile` runs only when the active profile **id** changes (startup, `SetActiveProfile`, delete-induced switch).
- Reads the profile from `IConfigurationStore.GetActiveProfile()` so hydration uses persisted state, not a stale `IActiveConfiguration` snapshot.
- Does **not** reset runtime toggles on `UpdateProfile`, keybind edits, or F11 save (avoids clobbering unsaved hotkey toggles).
- Config UI saves to the active profile apply toggles via the API `PUT /api/configs/{id}` handler calling `IFeatureState.ApplyFromProfile` directly.
- Logs the active profile name at information level after each apply.
- Unsubscribes on `StopAsync`.

## Dependencies

- `IFeatureState`
- `IConfigurationStore`
- `IConfigurationChangeNotifier`
- `ILogger<FeatureStateHydrator>`

## Configuration

None.
