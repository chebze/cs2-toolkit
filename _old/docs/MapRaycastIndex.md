# MapRaycastIndex

## Purpose

Spatially indexed triangle mesh for fast line and ground raycasts against a single map.

## Key API

| Method | Description |
|--------|-------------|
| `HasLineOfSight(start, end)` | True if no blocking geometry between points |
| `TryRaycast(start, direction, maxDistance, out hit)` | Closest triangle hit |
| `TryGroundIntersection(origin, direction, maxDistance, out hit)` | Ground/surface hit for ripples and landing |

## Behavior

- Builds uniform 3D grid over mesh bounds at construction
- Uses visit stamps to dedupe triangle tests per query
- Constructed from `MapCollisionMesh` by `MapVisibilityChecker`

## Dependencies

- `MapCollisionMesh` — vertices and indices from [MapPhysicsParser.md](MapPhysicsParser.md)
