# MemoryState

## Purpose

Immutable snapshot of game memory published on each `OnMemoryRead` event.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsAttached` | `bool` | Memory reader is connected to CS2 |
| `IsInGame` | `bool` | Local player pawn exists |
| `IsInMatch` | `bool` | Active match in progress |
| `LocalTeam` | `int` | Local player's team ID (2=T, 3=CT) |
| `Round` | `RoundInfo` | Round state from game rules |
| `EnemiesAlive` | `int` | Living enemies (from game rules, with player-list fallback) |
| `EnemiesDead` | `int` | Dead enemies (from player list) |
| `TeammatesAlive` | `int` | Living teammates excluding local player |
| `TeammatesDead` | `int` | Dead teammates excluding local player |
| `Bomb` | `BombInfo` | Bomb state (`Carried`, `Equipped`, `On ground`, `Planting`, `Planted`, `Defusing`, or hidden) |
| `ClairvoyanceTips` | `IReadOnlyList<string>` | Contextual advisor lines for the clairvoyance overlay |

## Stat sources

| Stat | Primary source | Fallback |
|------|----------------|----------|
| Alive counts | Player scan — pawn health for enemies, controller fields for teammates | Game rules stats |
| Dead counts | Player controller scan | — |

Alive counts from game rules are adjusted to exclude the local player from teammate totals.

## Match detection (`IsInMatch`)

`true` when local pawn exists, local team is T/CT, and `m_bHasMatchStarted` is set when game rules are readable.

When `IsInMatch` is `false`, stats are zero and `Players` is empty.

## Static instance

`MemoryState.Detached` — returned when not attached or on read failure.
