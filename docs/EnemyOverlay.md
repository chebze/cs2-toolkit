# EnemyOverlay

## Purpose

Draws **last-known enemy skeletons** on screen for the current round. When you or any teammate spots an enemy, their bone positions are saved and rendered as a world-space skeleton until the round resets.

## Behavior

- Layer: `enemy-last-seen` at z-index `100`
- Subscribes to `OnMemoryRead` for topmost refresh
- Reads snapshots from `EnemyLastSeenTracker`
- Projects bone connections with `LatestViewMatrix` and draws lines via `SkeletonDrawer`

## Round lifecycle

All last-seen data is cleared when `Round.RoundStartCount` changes (new round). Dead enemies are removed from tracking as soon as they are no longer alive.

## Configuration

`Toolkit:Overlay:EnemyLastSeen` in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `Color` | `#FF6B6B` | Skeleton line color |
| `LineWidth` | `1.5` | Skeleton line width in pixels |

## Related

- `EnemyLastSeenTracker` — spotted detection and bone capture
- `PlayerBones` — bone indices and skeleton connections
- `BoneHelper` — reads bone matrices from pawn scene nodes
