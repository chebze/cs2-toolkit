# BombSiteHelper

## Purpose

Internal helper for bomb site centers, planted C4 resolution, and entity world positions.

## Key API (representative)

| Method | Description |
|--------|-------------|
| `ReadEntityPosition(memory, offsets, entity)` | Feet/world origin |
| `TryReadSites(memory, offsets, entityList)` | A/B site centers |
| `ResolvePlantedC4Entity(...)` | Planted bomb entity pointer |
| `LabelForPosition(position, sites)` | Nearest site label |

## Dependencies

Used by [EntityResolver.md](EntityResolver.md), [EnemySoundTracker.md](EnemySoundTracker.md), [ClairvoyanceAdvisor.md](ClairvoyanceAdvisor.md), grenade and combat helpers.
