# IFeatureService

## Purpose

Contract for a toolkit combat or overlay feature driven by game snapshots.

## Key API

- `FeatureId Id`
- `bool IsEnabled`
- `void OnSnapshot(FeatureContext context)`

## Behavior

Implementations read runtime state from `IFeatureState`. `FeatureCoordinator` invokes features where `IsEnabled` is true each tick before overlay composition.

ESP tracker services (`EnemyEspFeatureService`, `SoundEspFeatureService`) return `IsEnabled => true` so they always receive ticks and can reset internal state when the feature is toggled off.

## Dependencies

- `FeatureContext`
- `IFeatureState`

## Configuration

Per-feature defaults arrive from profile settings in later migration phases.
