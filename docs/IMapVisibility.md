# IMapVisibility

## Purpose

Line-of-sight queries between world positions for ESP and aim features.

## Key API

- `bool HasLineOfSight(Vector3 from, Vector3 to)`

## Behavior

Implemented by `MapVisibilityService` using `MapRaycastIndex` BVH grids built from parsed collision meshes. Returns `true` when no mesh is available for the active map.

## Dependencies

- `MapDataService` preload at startup
- Active map set by `GameMemoryLoop` when attached

## Configuration

`Toolkit:Maps:CacheDirectory` and optional `Toolkit:Maps:MapsDirectory`.
