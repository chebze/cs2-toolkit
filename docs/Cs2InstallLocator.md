# Cs2InstallLocator

## Purpose

Discovers the CS2 installation `maps` directory on Windows.

## Key API

| Member | Description |
|--------|-------------|
| `LocateMapsDirectory()` | Returns `Cs2MapsLocation` or null |
| `EnumerateMapVpks(mapsDirectory)` | Yields `*_dir.vpk` paths |

## Behavior

- Probes Steam library paths and common install locations
- `Cs2MapsLocation` carries path and discovery source label for logging

## Configuration

Override with `Toolkit:Maps:MapsDirectory` on [MapDataService.md](MapDataService.md).
