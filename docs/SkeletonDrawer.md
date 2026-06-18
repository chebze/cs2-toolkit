# SkeletonDrawer

## Purpose

Internal helper that draws enemy skeleton lines and head/neck highlight from `EnemyLastSeenSnapshot` data.

## Key API

- `DrawLastSeen(graphics, snapshots, viewMatrix, screenWidth, screenHeight, color, lineWidth)`

## Behavior

- Projects `PlayerBones.Connections` as line segments
- Draws head circle and optional neck connector
- Skips invalid or off-screen bones

## Dependencies

- [EnemyOverlay.md](EnemyOverlay.md)
- [WorldToScreenHelper.md](WorldToScreenHelper.md)
