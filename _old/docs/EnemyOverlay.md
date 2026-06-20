# EnemyOverlay

## Purpose

Draws enemy skeleton overlays on screen. Mode is controlled by `EnemyEspState` (cycled with `F6` by default).

## Modes

| Mode | Data source | Behavior |
|------|-------------|----------|
| `Disabled` | — | Nothing drawn |
| `LastSeen` | `EnemyLastSeenTracker.CopyDrawableSnapshots()` | Last spotted positions until round reset |
| `Full` | `EnemyLastSeenTracker.CopyLiveSnapshots()` | Live skeletons for currently spotted enemies |

## Behavior

- Layer: `enemy-last-seen` at z-index `100`
- Subscribes to `OnMemoryRead` for topmost refresh
- Projects bone connections with `ViewMatrixHolder` and draws via `SkeletonDrawer`
- Skips drawing when not in match or ESP is disabled

## Round lifecycle

Last-seen data is cleared when `Round.RoundStartCount` changes. Dead enemies are removed from tracking as soon as they are no longer alive.

## Configuration

`Toolkit:Overlay:EnemyLastSeen` in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `Color` | `#FF6B6B` | Skeleton line color |
| `LineWidth` | `1.5` | Skeleton line width in pixels |

`Toolkit:EnemyEsp` controls toggle key and initial mode. Status label: [EnemyEspStatusOverlay.md](EnemyEspStatusOverlay.md).

## Related

- `EnemyLastSeenTracker` — spotted detection and bone capture
- `EnemyEspToggleService` — hotkey mode cycling
- `PlayerBones` — bone indices and skeleton connections
- `BoneHelper` — reads bone matrices from pawn scene nodes
