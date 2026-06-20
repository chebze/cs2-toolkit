# EnemyEspDrawBuilder

## Purpose

Builds overlay `DrawCommand` lists for enemy ESP skeletons, bounding boxes, and name/health labels.

## Key API

| Member | Description |
|--------|-------------|
| `Build(targets, options, projector, viewMatrix, width, height, zIndex)` | Returns draw commands for the given ESP targets |

## Behavior

- Draws head circle and bone connection lines (ported from legacy `SkeletonDrawer`)
- Skips bone segments longer than `PlayerBones.MaxConnectionWorldDistance`
- Optional axis-aligned bounding box from projected bone extents
- Optional name and health text above the head bone

## Dependencies

- `EspTarget`, `PlayerBones`, `EnemyEspProfileOptions`
- `IWorldProjector`, `OverlayColorParser`

## Configuration

Profile `EnemyEsp` options: `SkeletonColor`, `BoundingBoxColor`, `SkeletonLineWidth`, `ShowBoundingBox`, `ShowPlayerName`, `ShowPlayerHealth`.
