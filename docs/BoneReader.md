# BoneReader

## Purpose

Reads player skeleton bone world positions from CS2 process memory during snapshot collection.

## Key API

| Member | Description |
|--------|-------------|
| `TryReadSkeleton(memory, offsets, pawn)` | Returns `PlayerBones` when pelvis, neck, and head are valid; otherwise `null` |

## Behavior

- Follows scene node → model state → bone array pointer chain from the pawn entity
- Reads only `PlayerBones.RequiredIndices` using `PlayerBones.MatrixStride`
- Validates pelvis, neck, and head before returning a skeleton

## Dependencies

- `ProcessMemory`, `GameOffsets`
- `PlayerBones`, `BonePosition`, `Vector3`

## Configuration

None.
