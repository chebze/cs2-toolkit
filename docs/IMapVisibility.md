# IMapVisibility

## Purpose

Line-of-sight queries between world positions for ESP and aim features.

## Key API

- `bool HasLineOfSight(Vector3 from, Vector3 to)`

## Behavior

Phase 4 registers `MapVisibilityStub`, which always returns `true`. Real BVH raycasting is implemented in Phase 5.

## Dependencies

Phase 5: parsed map geometry and `MapRaycastIndex`.

## Configuration

`Toolkit:Maps` host settings (Phase 5).
