# TeammateOverlay

## Purpose

Subscribes to `OnMemoryRead` and renders teammate alive/dead counts on the `teammate-stats` overlay layer.

## Displayed stats

Only rendered when `MemoryState.IsInMatch` is `true`. Clears the layer in main menu and other non-match states. Calls `EnsureOnTop()` when drawing so the overlay stays visible over fullscreen CS2.

```
Teammates
  Alive: x
  Dead:  x
```

Counts use `MemoryState.TeammatesAlive` and `MemoryState.TeammatesDead`. The local player is excluded from teammate counts.

## Configuration

Controlled via `appsettings.json` → `Toolkit:Overlay:TeammateStats`:

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Set `false` to disable the overlay |
| `X` | 16 | Left position |
| `Y` | 120 | Top position |
| `Color` | `#6BCB77` | Text color (HTML hex) |
| `FontSize` | 14 | Font size in points |

## Layer

- Name: `teammate-stats`
- Z-Index: `100`

## Lifecycle

Registered as both a singleton and `IHostedService`. Mirrors `EnemyOverlay` architecture.
