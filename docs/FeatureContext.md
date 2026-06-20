# FeatureContext

## Purpose

Per-tick bundle passed to feature services: snapshot, resolved settings, and input port.

## Key API

- `GameSnapshot Snapshot`
- `ToolkitSettings Settings`
- `ResolvedWeaponSettings WeaponSettings`
- `IInputSimulator Input`

## Behavior

Built by `FeatureCoordinator` using the active weapon id from `LocalPlayer`.

## Dependencies

- `CS2Toolkit.Models.Abstractions`
- `CS2Toolkit.Configuration.Abstractions`
- `CS2Toolkit.Input.Abstractions`

## Configuration

Weapon layering via `IActiveConfiguration.ResolveWeapon`.
