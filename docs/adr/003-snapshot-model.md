# ADR 003: Mapped game snapshot model

## Status

Accepted

## Context

The legacy `EntityResolver` mixed raw memory layout, offset math, and feature-specific parsing in one large component. Services and overlays reached into memory structs and duplicated visibility/weapon logic.

## Decision

`CS2Toolkit.Game` owns all memory access and maps each tick to an immutable **`GameSnapshot`** (in `Models.Abstractions`):

- Game reads process memory, applies offsets internally, runs focused readers (players, bomb, grenades, radar, etc.)
- `GameSnapshot` exposes feature-ready views: `TriggerbotState`, `RcsState`, `AimHelperState`, `RadarSnapshot`, player list, local player, view matrix, etc.
- `GameStatePublisher` implements `IGameStateSource` / `IReadOnlyGameState` with a latest-wins slot
- **Services never parse offsets or raw structs** — they consume `GameSnapshot` via `FeatureContext`
- Overlays never read `GameSnapshot` directly; they receive data through presenters that run at compose time

Feature logic that needs historical state (enemy ESP last-seen, sound ESP waves) keeps trackers in Services keyed by snapshot content, not by re-reading memory.

## Consequences

**Positive**

- Services testable with fabricated snapshots
- Offset updates isolated to Game project
- Single mapping path reduces drift between features

**Negative**

- `GameSnapshot` grows as features add mapped fields
- Reader split inside Game is still incremental (`EntitySnapshotReader` interim monolith)

## Related

- [GameSnapshot.md](../GameSnapshot.md)
- [GameMemoryLoop.md](../GameMemoryLoop.md)
- [IFeatureService.md](../IFeatureService.md)
