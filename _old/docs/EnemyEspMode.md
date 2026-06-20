# EnemyEspMode

## Purpose

Enum and parser for enemy skeleton overlay modes.

## Values

| Value | Config string | Description |
|-------|---------------|-------------|
| `Disabled` | `Disabled` | No skeleton drawing |
| `LastSeen` | `LastSeen` | Last spotted positions |
| `Full` | `Full` | Live spotted enemies |

## Key API

- `EnemyEspModeParser.Parse(string?)` — reads config
- `EnemyEspModeParser.ToConfigValue(EnemyEspMode)` — writes config on save
