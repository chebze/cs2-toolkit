# MapVisibilityChecker

## Purpose

Registry of per-map `MapRaycastIndex` instances with active map selection for LOS and surface queries.

## Key API

| Method | Description |
|--------|-------------|
| `RegisterMesh(MapCollisionMesh)` | Adds or replaces index for map name |
| `SetActiveMap(string? mapName)` | Selects current map (normalized) |
| `HasLineOfSight(start, end)` | Active map LOS query |
| `TryRaycast(...)` / `TryGroundIntersection(...)` | Active map raycasts |
| `NormalizeMapName(string)` | Strips `de_`/`cs_` prefixes and workshop paths |

## Behavior

- Used by triggerbot, aim helper, grenade simulator, and ground ring projection
- Returns miss / clear LOS when no active map or no registered mesh

## Dependencies

- [MapRaycastIndex.md](MapRaycastIndex.md)
- Updated each memory tick by `GameMemoryReader` via [MapNameReader.md](MapNameReader.md)
