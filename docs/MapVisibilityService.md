# MapVisibilityService

## Purpose

Implements `IMapVisibility` using parsed map collision geometry and spatial raycast indices.

## Key API

Implements `IMapVisibility`.

- `HasLineOfSight(from, to)` — triangle mesh raycast on the active map
- `SetActiveMap(mapName)` — selects raycast index (called from `GameMemoryLoop`)
- `IsReady` / `LoadedMapCount` — diagnostics

## Behavior

- Delegates to internal `MapVisibilityChecker`
- Converts `CS2Toolkit.Models.Abstractions.Vector3` to `System.Numerics.Vector3` for ray tests
- Returns `true` when no mesh is loaded for the active map (graceful degradation)
- Active map updates when the in-game map name changes

## Dependencies

- `MapVisibilityChecker`
- `MapDataService` (registers meshes at startup)

## Configuration

None directly; depends on map preload completing via `MapPreloadHostedService`.
