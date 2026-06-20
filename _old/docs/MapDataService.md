# MapDataService

## Purpose

Loads CS2 map collision meshes at startup for raycast-based features.

## Key API

| Member | Description |
|--------|-------------|
| `VisibilityChecker` | Shared `MapVisibilityChecker` instance |
| `ParseAllMapsAsync(reportProgress, ct)` | Parse VPKs or load cache |

## Behavior

1. Resolves CS2 `maps` directory via [Cs2InstallLocator.md](Cs2InstallLocator.md) or `Toolkit:Maps:MapsDirectory`
2. Enumerates map VPKs and parses physics via `MapPhysicsParser`
3. Caches meshes under `Toolkit:Maps:CacheDirectory`
4. Falls back to cache when install path is missing
5. Signals `RuntimeGate.SignalMapParsingComplete()` when done

Non-fatal on failure — features degrade without meshes.

## Configuration

`Toolkit:Maps:CacheDirectory`, `Toolkit:Maps:MapsDirectory`

## Dependencies

- `MapPhysicsParser`, `MapVisibilityChecker`, `RuntimeGate`
