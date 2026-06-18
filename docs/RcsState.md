# RcsState

## Purpose

Thread-safe runtime enabled flag for recoil compensation.

## Key API

| Member | Description |
|--------|-------------|
| `IsEnabled` | Current state |
| `Initialize(RcsOptions)` | From config |
| `Toggle()` | Flip enabled |

## Configuration

`Toolkit:Rcs:Enabled` — persisted on save.
