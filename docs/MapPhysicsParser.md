# MapPhysicsParser

## Purpose

Extracts world collision triangle meshes from CS2 map VPK files using ValvePak and ValveResourceFormat.

## Key API

- `ParseMapVpk(vpkPath, cacheDirectory, mapsDirectory?)` — parse and cache mesh
- `LoadCachedMesh(cachePath)` — load binary `MAP1` cache file

## Behavior

- Reads `world_physics.vphys_c` / `world_physics.vmdl_c` from map VPK
- Falls back to `pak01_dir.vpk` when map VPK has no physics
- Writes best-effort `.mapmesh` cache beside other map data

## Dependencies

- NuGet: `ValvePak`, `ValveResourceFormat`
- `Microsoft.Extensions.Logging`

## Configuration

Cache path supplied by `MapDataService` from `Toolkit:Maps:CacheDirectory`.
