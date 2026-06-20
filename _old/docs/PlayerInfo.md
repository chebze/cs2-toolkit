# PlayerInfo

## Purpose

Represents a single player entity resolved from CS2 memory.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Index` | `int` | Entity list index (1–64) |
| `Name` | `string` | Player display name from controller |
| `Team` | `int` | Team number (0=none, 2=T, 3=CT) |
| `Health` | `int` | Current health (0–100) |
| `IsAlive` | `bool` | Derived from health and life state |
| `IsLocalPlayer` | `bool` | Whether this is the local player pawn |

## Usage

Included in `MemoryState.Players`. Used by stat aggregations and available for future per-player overlays (name tags, health bars, etc.).
