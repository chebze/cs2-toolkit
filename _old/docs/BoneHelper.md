# BoneHelper

## Purpose

Internal helper that reads required player bone world positions from a pawn's scene node.

## Key API

```csharp
bool TryReadSkeleton(ProcessMemory memory, GameOffsets offsets, nint pawn, Span<Vector3> bones)
```

## Behavior

- Resolves `m_pGameSceneNode` → model state bone array
- Fills `PlayerBones.RequiredIndices` into the span
- Returns true when pelvis, neck, and head are valid

## Dependencies

- Used by [EnemyLastSeenTracker.md](EnemyLastSeenTracker.md) and [AimHelper.md](AimHelper.md)
