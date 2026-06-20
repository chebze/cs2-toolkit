# PlayerBones

## Purpose

Immutable skeleton bone positions for a player, used by ESP and aim features.

## Key API

| Member | Description |
|--------|-------------|
| `RequiredIndices` | Bone indices read from game memory |
| `Connections` | Skeleton line pairs for overlay drawing |
| `HasValidSkeleton` | True when pelvis, neck, and head are valid |
| `TryGetBone(index, out position)` | Looks up a bone by animgraph index |

## Behavior

- `Count`, `MatrixStride`, and `MaxConnectionWorldDistance` match legacy animgraph layout
- `Empty` is a shared empty instance

## Dependencies

- `BoneId`, `BonePosition`, `Vector3`

## Configuration

None.
