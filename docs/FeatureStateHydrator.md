# FeatureStateHydrator

## Purpose

Keeps `IFeatureState` in sync with the active configuration profile on startup and after profile or settings changes.

## Key API

Hosted service (`IHostedService`); no public methods.

## Behavior

- On `StartAsync`, subscribes to `IConfigurationChangeNotifier.ConfigurationChanged` and immediately applies the current profile.
- `ApplyFromProfile` is invoked via `_featureState.ApplyFromProfile(_configuration.Current.Profile)`.
- Fires on profile switch (`SetActiveProfile`), API updates, F11 save round-trips, and initial load.
- Logs the active profile name at information level after each apply.
- Unsubscribes on `StopAsync`.

## Dependencies

- `IFeatureState`
- `IActiveConfiguration`
- `IConfigurationChangeNotifier`
- `ILogger<FeatureStateHydrator>`

## Configuration

None.
