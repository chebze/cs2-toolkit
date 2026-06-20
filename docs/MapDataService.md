# MapDataService

## Purpose

Discovers CS2 map VPKs, parses collision meshes, and registers them with `MapVisibilityChecker`. Falls back to cached `.mapmesh` files when VPKs are unavailable.

## Key API

- `ParseAllMapsAsync(reportProgress, cancellationToken)` — preload all playable maps
- `VisibilityChecker` — internal raycast index registry

## Behavior

- Resolves maps directory from `Toolkit:Maps:MapsDirectory`, running `cs2` process, Steam libraries, or registry
- Enumerates `de_` / `cs_` / `ar_` map VPKs via `Cs2InstallLocator`
- Parses or loads cache under `Toolkit:Maps:CacheDirectory` (default `data/maps` relative to app base)
- Non-fatal: continues without meshes when discovery or parsing fails

## Dependencies

- `MapPhysicsParser`
- `MapVisibilityChecker`
- `ToolkitHostSettings`

## Configuration

```json
"Toolkit": {
  "Maps": {
    "CacheDirectory": "data/maps",
    "MapsDirectory": null
  }
}
```

Set `MapsDirectory` to override auto-discovery.
