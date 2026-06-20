# IFeatureService

## Purpose

Contract for a toolkit combat or overlay feature driven by game snapshots.

## Key API

- `FeatureId Id`
- `bool IsEnabled`
- `void OnSnapshot(FeatureContext context)`

## Behavior

Implementations read runtime state from `IFeatureState`. `FeatureCoordinator` invokes enabled features each tick before overlay composition.

## Dependencies

- `FeatureContext`
- `IFeatureState`

## Configuration

Per-feature defaults arrive from profile settings in later migration phases.
