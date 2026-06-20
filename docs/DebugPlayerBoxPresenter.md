# DebugPlayerBoxPresenter

## Purpose

Phase 6 debug overlay layer drawing world-projected bounding boxes for alive players from `GameSnapshot`.

## Key API

Implements `IOverlayPresenter` with layer name `debug-player-boxes`.

## Behavior

- Disabled unless `Toolkit:ShowDebugPlayerBoxes` is `true` (default `false`)
- Skips local player and players without `WorldPosition`
- Projects feet + estimated head height to screen for dynamic box size
- Team-colored rectangle and name/health label

## Dependencies

- `IWorldProjector`
- `GameSnapshot.ViewMatrix` and `Player.WorldPosition`

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `Toolkit:ShowDebugPlayerBoxes` | `false` | Draw team-colored debug boxes for all players |
