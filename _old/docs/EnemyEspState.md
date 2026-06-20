# EnemyEspState

## Purpose

Thread-safe runtime state for enemy skeleton ESP mode.

## Key API

| Member | Description |
|--------|-------------|
| `Mode` | Current `EnemyEspMode` |
| `Initialize(EnemyEspOptions)` | Sets mode from config |
| `CycleMode()` | Rotates Disabled → LastSeen → Full → Disabled |

## Behavior

Uses `Interlocked.CompareExchange` for lock-free mode cycling from `EnemyEspToggleService`.

## Configuration

`Toolkit:EnemyEsp:Mode` — initial mode. Persisted on save via [SettingsSaveService.md](SettingsSaveService.md).
