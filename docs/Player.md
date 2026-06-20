# Player

## Purpose

Mapped player entry on `GameSnapshot.Players`.

## Key API

Record fields: `Id`, `Name`, `Team`, `Health`, `IsAlive`, `IsLocalPlayer`, `WorldPosition`, `Bones`, `IsSpottedByTeam`, `IsVisibleToLocalPlayer`.

## Behavior

- Core fields come from controller/pawn memory reads in `EntitySnapshotReader`
- ESP fields (`Bones`, spotted flags) are enriched after player collection in the Game layer
- Services consume this shape only; no direct memory access

## Dependencies

- `PlayerId`, `Team`, `Vector3`, `PlayerBones`

## Configuration

None.
