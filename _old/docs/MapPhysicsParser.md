# MapPhysicsParser

## Purpose

Extracts collision triangle meshes from CS2 map VPK files using ValvePak and ValveResourceFormat.

## Key API

| Method | Description |
|--------|-------------|
| `ParseMapAsync(vpkPath, cancellationToken)` | Returns `MapCollisionMesh` or null |

## Behavior

- Reads physics/collision resources from map packages
- Produces world-space vertices and triangle indices
- Invoked by [MapDataService.md](MapDataService.md) for each discovered VPK
- Results cached to disk under `Toolkit:Maps:CacheDirectory`

## Dependencies

- NuGet: `ValvePak`, `ValveResourceFormat`
