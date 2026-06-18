# BombInfo

## Purpose

Detailed bomb state published in `MemoryState.Bomb` on each memory read.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Status` | `BombStatus` | Current bomb state |
| `Site` | `string?` | Bombsite label (`A`, `B`) when planting or planted |
| `TimeLeftSeconds` | `int?` | Seconds until detonation when planted |
| `HasDefuseKit` | `bool?` | Whether the defuser has a kit |
| `DefuseTimeSeconds` | `int?` | Seconds until defuse completes |
| `WillDefuseSucceed` | `bool?` | Whether the defuse finishes before detonation |

## Visibility

`IsVisible` is `true` when `Status` is not `None`.

`BombInfo.Hidden` is the default when bomb state is unknown.
