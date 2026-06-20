# IMapCatalog

## Purpose

Exposes the current in-game map name when attached.

## Key API

- `string? CurrentMap`

## Behavior

Implemented by `MapCatalogService`, which reads the map name via internal `MapNameReader`. Full map metadata and parsing arrive in Phase 5.

## Dependencies

- `ProcessMemory`, loaded offsets (internal)

## Configuration

None.
